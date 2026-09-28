"""Import QA vendor JARs into local Maven using their STC coordinates; never deploy."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
NAMESPACE = 'http://maven.apache.org/POM/4.0.0'
NS = {'m': NAMESPACE}
ET.register_namespace('', NAMESPACE)


def child(parent, name, text=None):
    element = ET.SubElement(parent, '{' + NAMESPACE + '}' + name)
    element.text = text
    return element


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--qa-lib', type=Path, required=True)
    parser.add_argument('--stc-parent-pom', type=Path, required=True)
    parser.add_argument('--root-pom', type=Path,
                        help='Original mvn-root POM, if available')
    parser.add_argument('--reconstruct-root', action='store_true',
                        help='Use the explicitly marked local root when the original is absent')
    parser.add_argument('--maven', default='mvn')
    parser.add_argument('--local-repository', type=Path,
                        default=Path.home() / '.m2/repository')
    args = parser.parse_args()
    output = ROOT / 'runtime/qa-maven-bootstrap'
    output.mkdir(parents=True, exist_ok=True)
    project = ET.Element('{' + NAMESPACE + '}project')
    for name, value in [('modelVersion', '4.0.0'), ('groupId', 'cgws.ficc.saletrading'),
                        ('artifactId', 'qa-library-import'), ('version', '1.0'),
                        ('packaging', 'pom')]:
        child(project, name, value)
    plugin = child(child(child(project, 'build'), 'plugins'), 'plugin')
    child(plugin, 'groupId', 'org.apache.maven.plugins')
    child(plugin, 'artifactId', 'maven-install-plugin')
    child(plugin, 'version', '3.1.3')
    executions = child(plugin, 'executions')
    manifest = []

    def install(file, group, artifact, version, pom=None, classifier=''):
        file = file.resolve(strict=True)
        execution = child(executions, 'execution')
        child(execution, 'id', 'import-' + str(len(manifest)))
        child(execution, 'phase', 'validate')
        child(child(execution, 'goals'), 'goal', 'install-file')
        config = child(execution, 'configuration')
        for name, value in [('file', str(file)), ('groupId', group), ('artifactId', artifact),
                            ('version', version), ('packaging', 'jar' if file.suffix == '.jar' else 'pom'),
                            ('localRepositoryPath', str(args.local_repository.resolve()))]:
            child(config, name, value)
        if pom:
            child(config, 'pomFile', str(pom.resolve()))
        else:
            child(config, 'generatePom', 'true')
        if classifier:
            child(config, 'classifier', classifier)
        manifest.append({'file': str(file), 'coordinates': ':'.join([group, artifact, version]),
                         'sha256': hashlib.sha256(file.read_bytes()).hexdigest(),
                         'pom': str(pom) if pom else 'install-file generated metadata'})

    cached_root = args.local_repository / 'sumscope/mvn-root/1.0.0/mvn-root-1.0.0.pom'
    if args.root_pom:
        install(args.root_pom, 'sumscope', 'mvn-root', '1.0.0')
    elif not cached_root.exists():
        if not args.reconstruct_root:
            parser.error('Supply --root-pom or explicitly select --reconstruct-root')
        install(ROOT / 'build/qa-maven/mvn-root.pom.xml', 'sumscope', 'mvn-root', '1.0.0')
    install(args.stc_parent_pom, 'sumscope.stc', 'sc-app-stc-parent-pom', '0.1.1-SNAPSHOT')
    dependencies = set()
    for artifact, version in [('sc-app-stc', '0.1.1-SNAPSHOT'), ('sc-app-common', '0.1.0-SNAPSHOT')]:
        jar = args.qa_lib / (artifact + '-' + version + '.jar')
        with zipfile.ZipFile(jar) as archive:
            data = archive.read('META-INF/maven/sumscope.stc/' + artifact + '/pom.xml')
        pom = output / (artifact + '.pom.xml')
        pom.write_bytes(data)
        install(jar, 'sumscope.stc', artifact, version, pom)
        document = ET.fromstring(data)
        properties = {entry.tag.split('}')[-1]: entry.text
                      for entry in document.find('m:properties', NS)}
        for dependency in document.findall('m:dependencies/m:dependency', NS):
            values = [dependency.findtext('m:' + key, '', NS)
                      for key in ['groupId', 'artifactId', 'version', 'classifier']]
            for key, value in properties.items():
                values[2] = values[2].replace('${' + key + '}', value)
            if values[0].startswith(('ats.', 'net.forexstone', 'com.cfets', 'com.oracle', 'sumscope')):
                dependencies.add(tuple(values))
    for group, artifact, version, classifier in sorted(dependencies):
        if artifact in ('sc-app-stc', 'sc-app-common'):
            continue
        name = artifact + '-' + version + ('-' + classifier if classifier else '') + '.jar'
        install(args.qa_lib / name, group, artifact, version, classifier=classifier)
    ET.indent(project, space='    ')
    ET.ElementTree(project).write(output / 'pom.xml', encoding='utf-8', xml_declaration=True)
    (output / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    environment = dict(os.environ)
    environment.setdefault('MAVEN_OPTS', '-Xmx256m -XX:ActiveProcessorCount=2')
    subprocess.run([args.maven, '-B', '-ntp', '-f', str(output / 'pom.xml'), 'validate'],
                   env=environment, check=True)
    print('Imported', len(manifest), 'QA artifacts. See runtime/qa-maven-bootstrap/manifest.json.')


if __name__ == '__main__':
    main()
