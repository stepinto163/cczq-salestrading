"""Verify service distributions, manifest dependencies and Java 8 bytecode without starting services."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--stc', action='store_true', help='Check standard STC target instead of target-compat')
    options = parser.parse_args()
    output = 'target' if options.stc else 'target-compat'
    reports = []
    common = ROOT / 'cgws-ficc-saletrading-common' / output / 'cgws-ficc-saletrading-common-0.1.0-SNAPSHOT.jar'
    common_hash = hashlib.sha256(common.read_bytes()).digest()
    for service, main_class in [('quotesvr', 'ficc.quote.Main'), ('notificationsvr', 'ficc.notification.NotificationMain')]:
        module = 'cgws-ficc-saletrading-' + service
        package_path = ROOT / module / output / (module + '-0.1.0-SNAPSHOT-dist.zip')
        prefix = module + '/'
        with zipfile.ZipFile(package_path) as package:
            names = package.namelist()
            for required in ['control/ATSConfig.ini', 'sql/001-create-tables.sql', 'config/database.properties.example']:
                assert prefix + required in names, required
            assert not any(name.endswith(('.lic', '.local.properties', '.log')) for name in names)
            executable_name = prefix + module + '-0.1.0-SNAPSHOT.jar'
            with zipfile.ZipFile(io.BytesIO(package.read(executable_name))) as executable:
                manifest = executable.read('META-INF/MANIFEST.MF').decode().replace('\r\n', '\n').replace('\n ', '')
                attributes = dict(line.split(': ', 1) for line in manifest.splitlines() if ': ' in line)
                assert attributes['Main-Class'] == main_class
                dependencies = attributes['Class-Path'].split()
                expected = {prefix + name for name in dependencies}
                actual = {name for name in names if name.startswith(prefix + 'lib/') and name.endswith('.jar')}
                assert expected == actual, 'Manifest and packaged dependencies differ'
            assert hashlib.sha256(package.read(prefix + 'lib/' + common.name)).digest() == common_hash
            checked_classes = 0
            for filename in [executable_name] + sorted(expected):
                with zipfile.ZipFile(io.BytesIO(package.read(filename))) as jar:
                    for entry in jar.namelist():
                        if not entry.endswith('.class') or entry.startswith('META-INF/versions/') or entry.endswith('module-info.class'):
                            continue
                        major = struct.unpack('>H', jar.read(entry)[6:8])[0]
                        assert major <= 52, 'Not Java 8 compatible: ' + filename + ':' + entry
                        checked_classes += 1
            reports.append({'module': module, 'result': 'PASS', 'runtimeJars': len(expected), 'classes': checked_classes})
    report = {'result': 'PASS', 'packages': reports, 'servicesStarted': 0, 'quotesCreated': 0}
    destination = ROOT / 'runtime/maven-package-check.json'
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()
