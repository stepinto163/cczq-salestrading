"""Check deployment ZIP contents and ordered dependencies without starting services."""
import hashlib
import io
import json
from pathlib import Path
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    manifest = json.loads((ROOT / "build/foundation-artifacts.json").read_text(encoding="utf-8"))
    artifacts = manifest["artifacts"]
    foundation = [artifact for artifact in artifacts if artifact["file"] != "gw-local.jar"]
    gateway = next(artifact for artifact in artifacts if artifact["file"] == "gw-local.jar")
    reports = []
    for module, artifact_id, main_class in [
        ("cczq-app-salestrading-quotesvr", "cczq-app-salestrading-quotesvr", "ficc.quote.Main"),
        ("cczq-app-salestrading-notificationsvr", "cczq-app-salestrading-notificationsvr",
         "ficc.notification.NotificationMain"),
        ("gateway", "cczq-app-salestrading-gw", "GW"),
    ]:
        target = ROOT / module / "target-maven"
        packages = list(target.glob("*-dist.zip"))
        assert len(packages) == 1, "Expected one package: " + module
        expected = {}
        if module == "gateway":
            runtime = [gateway] + foundation
        else:
            common_jars = list((ROOT / "cczq-app-salestrading-common/target-maven").glob("*.jar"))
            assert len(common_jars) == 1
            common = common_jars[0]
            expected["lib/" + common.name] = digest(common.read_bytes())
            runtime = foundation
        for artifact in runtime:
            filename = artifact["artifactId"] + "-" + artifact["version"] + ".jar"
            expected["lib/" + filename] = artifact["sha256"]
        with zipfile.ZipFile(packages[0]) as package:
            root = artifact_id + "/"
            names = set(package.namelist())
            assert not any(name.endswith((".lic", ".local.properties", ".log")) for name in names)
            assert root + "control/ATSConfig.ini" in names
            assert root + "sql/001-create-demo-tables.sql" in names
            assert root + "config/database.properties.example" in names
            jars = [name for name in names if name.startswith(root)
                    and "/" not in name[len(root):] and name.endswith(".jar")]
            assert len(jars) == 1
            with zipfile.ZipFile(io.BytesIO(package.read(jars[0]))) as executable:
                text = executable.read("META-INF/MANIFEST.MF").decode().replace("\r\n", "\n")
                text = text.replace("\n ", "")
                attributes = dict(line.split(": ", 1) for line in text.splitlines() if ": " in line)
                assert attributes["Main-Class"] == main_class
                assert attributes["Class-Path"].split() == list(expected), "Classpath order changed"
                if module != "gateway":
                    assert main_class.replace(".", "/") + ".class" in executable.namelist()
            for filename, checksum in expected.items():
                assert digest(package.read(root + filename)) == checksum, "Changed JAR: " + filename
            if module == "gateway":
                gateway_name = "lib/" + gateway["artifactId"] + "-" + gateway["version"] + ".jar"
                with zipfile.ZipFile(io.BytesIO(package.read(root + gateway_name))) as library:
                    assert "GW.class" in library.namelist()
        reports.append({"module": module, "result": "PASS", "runtimeJars": len(expected)})
    report = {"result": "PASS", "packages": reports, "servicesStarted": 0, "quotesCreated": 0}
    report_path = ROOT / "runtime/maven-package-check.json"
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
