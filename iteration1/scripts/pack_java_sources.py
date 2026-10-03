"""Create a complete source ZIP for cgws-ficc Java projects, without build/runtime files."""

import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import zipfile


EXCLUDED_DIRECTORIES = {
    '.git', '.idea', '.vs', '__pycache__', 'node_modules', 'runtime',
    'logs', 'log', 'obj', 'out', 'dist', 'data',
}
EXCLUDED_SUFFIXES = {
    '.class', '.jar', '.war', '.ear', '.zip', '.gz', '.7z', '.tar',
    '.exe', '.dll', '.pdb', '.log', '.pid', '.dump', '.hprof', '.iml',
    '.lic', '.pem', '.key', '.pfx', '.p12', '.jks', '.keystore',
    '.backup', '.bak', '.tmp',
}
SECRET_ASSIGNMENT = re.compile(
    r'''(?im)^[ \t]*["']?([\w.\-]*(?:password|passwd|pwd|secret|token|api[_-]?key)[\w.\-]*)["']?[ \t]*[:=][ \t]*([^\r\n]*)$'''
)
CONFIG_SUFFIXES = {'.properties', '.ini', '.cfg', '.json', '.yaml', '.yml', '.xml'}


def git(project, *arguments):
    process = subprocess.run(
        ['git', '-C', str(project), *arguments],
        stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False,
    )
    if process.returncode:
        raise RuntimeError('Git could not list source files in ' + str(project))
    return process.stdout


def exclusion(relative):
    parts = Path(relative).parts
    name = parts[-1].lower()
    for part in parts[:-1]:
        lower = part.lower()
        if lower in EXCLUDED_DIRECTORIES or lower == 'target' or lower.startswith('target-'):
            return 'build/runtime directory'
    # deployment/bin/start and build/*.xml are source files, not compiled output.
    if parts[0].lower() == 'bin':
        return 'compiled output directory'
    if Path(name).suffix in EXCLUDED_SUFFIXES:
        return 'binary/build result/local artifact'
    if name.endswith('.local.properties') or (name.startswith('.env') and name != '.env.example'):
        return 'local configuration'
    if name in {'dbpoolconfig.ini', 'dbconfig.ini', 'qtintakeauth.properties', 'apikeylist.csv'}:
        return 'local credentials/connection configuration'
    return None


def credential_reason(path, content):
    text = content.decode('utf-8-sig', errors='replace')
    if re.search(r'-----BEGIN (?:[A-Z]+ )?PRIVATE KEY-----', text):
        return 'private key material'
    if path.suffix.lower() in CONFIG_SUFFIXES:
        for key, value in SECRET_ASSIGNMENT.findall(text):
            if key.lower().endswith('env'):
                continue
            value = value.strip().rstrip(',').strip().strip('\"\'')
            if not value or value.lower() in {'null', 'none', 'true', 'false', 'changeme', 'change-me'}:
                continue
            if value.startswith(('${', '$env:', '$ATS_ROOT', '<', '@', '%')):
                continue
            return 'possible literal credential in configuration; transfer separately after review'
    return None


def package(project_root, output=None):
    project_root = Path(project_root).resolve()
    projects = sorted(
        path for path in project_root.glob('cgws-ficc*')
        if path.is_dir() and (path / 'pom.xml').is_file()
    )
    if not projects:
        raise RuntimeError('No cgws-ficc* Java projects with pom.xml found in ' + str(project_root))
    if output is None:
        timestamp = datetime.now().strftime('%Y%m%d-%H%M%S-%f')
        output = project_root / 'runtime/source-sync' / ('cgws-ficc-java-sources-' + timestamp + '.zip')
    output = Path(output).resolve()
    if output.exists():
        raise RuntimeError('Output already exists: ' + str(output))
    output.parent.mkdir(parents=True, exist_ok=True)
    manifest = {
        'formatVersion': 1,
        'createdAtUtc': datetime.now(timezone.utc).isoformat(),
        'projects': [project.name for project in projects],
        'selection': 'All existing tracked and nonignored untracked files, using working-copy contents',
        'files': [],
        'excluded': [],
    }
    descriptor, temporary = tempfile.mkstemp(prefix='java-sources-', suffix='.tmp', dir=output.parent)
    os.close(descriptor)
    try:
        with zipfile.ZipFile(temporary, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            for project in projects:
                listed = git(project, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
                relative_paths = sorted({item.decode('utf-8') for item in listed.split(b'\0') if item})
                for relative in relative_paths:
                    path = project / relative
                    entry_name = project.name + '/' + relative
                    reason = exclusion(relative)
                    if path.is_symlink() or not path.resolve().is_relative_to(project.resolve()):
                        reason = 'link or path outside project'
                    if reason:
                        manifest['excluded'].append({'path': entry_name, 'reason': reason})
                        continue
                    if not path.is_file():
                        continue
                    if path.stat().st_size > 20 * 1024 * 1024:
                        raise RuntimeError('Unexpected source file over 20 MB: ' + entry_name)
                    content = path.read_bytes()
                    reason = credential_reason(path, content)
                    if reason:
                        manifest['excluded'].append({'path': entry_name, 'reason': reason})
                        continue
                    archive.writestr(entry_name, content)
                    manifest['files'].append({
                        'path': entry_name,
                        'size': len(content),
                        'sha256': hashlib.sha256(content).hexdigest(),
                    })
            for entry in manifest['files']:
                path = project_root / entry['path']
                if not path.exists() or hashlib.sha256(path.read_bytes()).hexdigest() != entry['sha256']:
                    raise RuntimeError('Source changed during packaging; retry: ' + entry['path'])
            archive.writestr('__source_sync__/manifest.json', json.dumps(manifest, ensure_ascii=False, indent=2))
            archive.writestr('__source_sync__/README.zh-CN.md', PACKAGE_README)
        os.replace(temporary, output)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)
    return output, manifest


PACKAGE_README = '''# Java 全量源码同步包

包含目录名以 cgws-ficc 开头的 Maven 项目，使用当前工作区完整文件内容，包含尚未提交的新文件和修改。并非仅打包 git diff，也不是可运行发布包。

- 包含 src、主 POM/兼容 POM、Jenkinsfile、control、配置模板、salt-formula、build 定义、deployment/bin 启动脚本和文档。
- 遵循 Git ignore，并排除编译结果、依赖 JAR、运行数据、日志、IDE 缓存和本机凭据配置。manifest.json 列出包内文件及额外排除项；Git 本身忽略的文件不逐项列出。
- 不删除、不清理、不修改源目录，也不提交或推送。
- 接收端先解压到新目录再合并，保留自己机器的配置／凭据。如果仅覆盖旧目录，旧版本已删除的文件仍然存在，不能视为严格全量替换。包内无 .git，不覆盖接收端 Git 历史。
- 先安装 parent-pom，再安装 common，最后构建各服务。本机与 STC/Jenkins 统一使用主 pom.xml 和 QA 原版依赖。
- 包不包含本机 Maven 仓库或兼容依赖；接收端需要已有对应依赖环境。
- 凭据检测是辅助检查；分享前检查排除列表及内容，尤其是手工写入源码或文档的信息。
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--project-root', type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument('--output', type=Path)
    arguments = parser.parse_args()
    output, manifest = package(arguments.project_root, arguments.output)
    print('ZIP:', output)
    print('Projects:', ', '.join(manifest['projects']))
    print('Source files:', len(manifest['files']))
    print('Additional exclusions:', len(manifest['excluded']))
    for entry in manifest['excluded']:
        print('  EXCLUDED:', entry['path'], '-', entry['reason'])
    print('Original source and build directories were not changed.')


if __name__ == '__main__':
    try:
        main()
    except (OSError, RuntimeError, UnicodeError) as error:
        print('ERROR:', str(error), file=sys.stderr)
        sys.exit(1)
