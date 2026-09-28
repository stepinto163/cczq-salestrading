"""Check standard STC service outputs without starting services."""
import hashlib
import json
from pathlib import Path
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def main():
    common = ROOT / 'cgws-ficc-saletrading-common/target/cgws-ficc-saletrading-common-0.1.0-SNAPSHOT.jar'
    common_hash = hashlib.sha256(common.read_bytes()).digest()
    reports = []
    for module, main_class in [('quotesvr', 'QuoteSvr'), ('notificationsvr', 'NotificationSvr'),
                               ('crmsvr', 'CRMSvr'), ('qtintakesvr', 'QtIntakeSvr')]:
        artifact = 'cgws-ficc-saletrading-' + module
        target = ROOT / artifact / 'target'
        executable = target / (artifact + '-0.1.0-SNAPSHOT.jar')
        library = target / 'javalib'
        assert (target / 'bin/start').is_file(), artifact + ': missing start script'
        assert hashlib.sha256((library / common.name).read_bytes()).digest() == common_hash
        with zipfile.ZipFile(executable) as archive:
            text = archive.read('META-INF/MANIFEST.MF').decode().replace('\r\n', '\n').replace('\n ', '')
            manifest = dict(line.split(': ', 1) for line in text.splitlines() if ': ' in line)
            assert manifest['Main-Class'] == main_class
            for dependency in manifest.get('Class-Path', '').split():
                assert (library / dependency).is_file(), artifact + ': missing ' + dependency
            for name in archive.namelist():
                if name.endswith('.class'):
                    assert struct.unpack('>H', archive.read(name)[6:8])[0] <= 52, name
        reports.append({'module': module, 'result': 'PASS', 'runtimeJars': len(list(library.glob('*.jar')))})
    report = {'result': 'PASS', 'packages': reports, 'servicesStarted': 0}
    (ROOT / 'runtime/maven-package-check.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()
