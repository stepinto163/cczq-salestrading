"""Register the verified legacy STC JAR set as ordinary Maven artifacts.

This is a one-time dependency import. It does not rebuild or modify legacy sources.
Use --deploy-url only when intentionally publishing to an internal Maven repository.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
GROUP_ID = "cczq.salestrading.compat"
NAMESPACE = "http://maven.apache.org/POM/4.0.0"
ET.register_namespace("", NAMESPACE)


def element(parent, name, text=None):
    child = ET.SubElement(parent, "{" + NAMESPACE + "}" + name)
    if text is not None:
        child.text = str(text)
    return child


def project(artifact, version, packaging):
    root = ET.Element("{" + NAMESPACE + "}project")
    element(root, "modelVersion", "4.0.0")
    element(root, "groupId", GROUP_ID)
    element(root, "artifactId", artifact)
    element(root, "version", version)
    element(root, "packaging", packaging)
    return root


def write_xml(path, root):
    ET.indent(root, space="    ")
    ET.ElementTree(root).write(path, encoding="utf-8", xml_declaration=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--lib", type=Path, default=ROOT / "runtime/foundation-build/lib")
    parser.add_argument("--maven", default="mvn")
    parser.add_argument("--local-repository", type=Path)
    parser.add_argument("--settings", type=Path)
    parser.add_argument("--deploy-url")
    parser.add_argument("--repository-id", default="ficc-internal")
    arguments = parser.parse_args()
    manifest = json.loads((ROOT / "build/foundation-artifacts.json").read_text(encoding="utf-8"))
    library = arguments.lib.resolve()
    artifacts = manifest["artifacts"]

    # Check every input before installing or uploading anything.
    for artifact in artifacts:
        path = library / artifact["file"]
        if not path.is_file():
            raise SystemExit("Missing dependency: " + str(path))
        with path.open("rb") as stream:
            actual = hashlib.file_digest(stream, "sha256").hexdigest()
        if actual != artifact["sha256"]:
            raise SystemExit("Dependency checksum mismatch: " + str(path))

    output = ROOT / "runtime/maven-bootstrap"
    output.mkdir(parents=True, exist_ok=True)
    bootstrap = project("foundation-bootstrap", manifest["runtimeVersion"], "pom")
    plugins = element(element(bootstrap, "build"), "plugins")
    plugin = element(plugins, "plugin")
    element(plugin, "groupId", "org.apache.maven.plugins")
    deploy = bool(arguments.deploy_url)
    element(plugin, "artifactId", "maven-deploy-plugin" if deploy else "maven-install-plugin")
    element(plugin, "version", "3.1.3")
    executions = element(plugin, "executions")

    def add_execution(identifier, artifact_file, pom_file):
        execution = element(executions, "execution")
        element(execution, "id", identifier)
        element(execution, "phase", "validate")
        element(element(execution, "goals"), "goal", "deploy-file" if deploy else "install-file")
        configuration = element(execution, "configuration")
        element(configuration, "file", artifact_file)
        element(configuration, "pomFile", pom_file)
        element(configuration, "generatePom", "false")
        if deploy:
            element(configuration, "url", arguments.deploy_url)
            element(configuration, "repositoryId", arguments.repository_id)

    for artifact in artifacts:
        pom = output / (artifact["artifactId"] + ".pom")
        write_xml(pom, project(artifact["artifactId"], artifact["version"], "jar"))
        add_execution(artifact["artifactId"], library / artifact["file"], pom)

    runtime_pom = ROOT / "build/stc-foundation-runtime.pom.xml"
    add_execution("stc-foundation-runtime", runtime_pom, runtime_pom)
    bootstrap_pom = output / "pom.xml"
    write_xml(bootstrap_pom, bootstrap)
    maven = Path(arguments.maven)
    executable = str(maven.resolve()) if maven.is_file() else arguments.maven
    command = [executable, "-B", "-ntp", "-f", str(bootstrap_pom)]
    if arguments.local_repository:
        command.append("-Dmaven.repo.local=" + str(arguments.local_repository.resolve()))
    if arguments.settings:
        command.extend(["-s", str(arguments.settings.resolve())])
    command.append("validate")
    environment = dict(os.environ)
    environment.setdefault("MAVEN_OPTS", "-Xmx384m -XX:ActiveProcessorCount=2")
    print("Verified", len(artifacts), "JARs; registering", manifest["runtimeVersion"], flush=True)
    subprocess.run(command, env=environment, check=True, timeout=600)


if __name__ == "__main__":
    main()
