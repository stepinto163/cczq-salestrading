"""Compile and test the ParserSvr adapter against QA jars; never deploy or start services."""
import argparse
from pathlib import Path
import subprocess
import os


def run(arguments, directory, log, timeout=45):
    with log.open('w', encoding='utf-8') as output:
        process = subprocess.Popen(arguments, cwd=directory, stdout=output, stderr=subprocess.STDOUT)
        try:
            code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            if os.name == 'nt':
                subprocess.run(['taskkill', '/PID', str(process.pid), '/T', '/F'], capture_output=True)
            else:
                process.kill()
                process.wait()
            raise RuntimeError('Watchdog expired: ' + str(log))
    if code:
        raise RuntimeError('Failed; see ' + str(log))


def main():
    options = argparse.ArgumentParser(description=__doc__)
    options.add_argument('--runtime', type=Path, required=True)
    options.add_argument('--java-home', type=Path, required=True)
    options.add_argument('--local-database-config', type=Path)
    args = options.parse_args()
    module = Path(__file__).resolve().parents[1]
    service = module
    runtime = args.runtime.resolve()
    java = args.java_home.resolve() / 'bin'
    suffix = '.exe' if os.name == 'nt' else ''
    output = module / 'target/service-check'
    # A fresh folder avoids stale classes, while retaining earlier reports.
    from datetime import datetime
    output = output / datetime.now().strftime('%Y%m%d-%H%M%S-%f')
    classes = output / 'classes'
    classes.mkdir(parents=True)
    sources = list((service / 'src/main/java').rglob('*.java'))
    sources += list((service / 'thirdparty').rglob('*.java'))
    tests = service / 'src/test/java/sc/app/stc/parser/utils'
    for name in ['JavaParserIntegrationTest', 'FiccCrmProtocolTest', 'JdbcParserReadOnlyCheck']:
        sources.append(tests / (name + '.java'))
    source_list = output / 'sources.txt'
    source_list.write_text('\n'.join('"' + path.as_posix() + '"' for path in sources), encoding='utf-8')
    # Compile both engine and host from this module; never use a stale engine JAR.
    libraries = [path for path in (runtime / 'javalib').glob('*.jar')
                 if not path.name.startswith(('cgws-ficc-parser-', 'cgws-ficc-parsersvr-', 'sc-app-stc-parsersvr-'))]
    classpath = os.pathsep.join(map(str, [classes] + libraries))
    run([str(java / ('javac' + suffix)), '-J-Xmx256m', '-J-XX:ActiveProcessorCount=1',
         '-encoding', 'UTF-8', '-source', '8', '-target', '8', '-cp', classpath,
         '-d', str(classes), '@' + str(source_list)], output, output / 'compile.log')
    command = [str(java / ('java' + suffix)), '-Xmx192m', '-XX:ActiveProcessorCount=1',
               '-Dfile.encoding=UTF-8', '-Djava.library.path=' + str(output), '-cp', classpath]
    run(command + ['org.junit.runner.JUnitCore',
                   'sc.app.stc.parser.utils.JavaParserIntegrationTest',
                   'sc.app.stc.parser.utils.FiccCrmProtocolTest'], output, output / 'contract.log', 20)
    if args.local_database_config:
        run(command + ['sc.app.stc.parser.utils.JdbcParserReadOnlyCheck',
                       str(args.local_database_config.resolve())], output, output / 'jdbc.log', 20)
    print('PASS. Reports: ' + str(output))
    print('No deployment. Use the service Maven build for release packaging.')


if __name__ == '__main__':
    main()
