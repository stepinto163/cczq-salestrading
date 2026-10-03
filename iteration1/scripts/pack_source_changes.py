"""Package Git working-copy source changes without committing or altering files."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile
from datetime import datetime, timezone
import zipfile


SOURCE_SUFFIXES = {
    '.java', '.cs', '.xaml', '.resx', '.resw', '.csproj', '.sln', '.props',
    '.targets', '.config', '.xml', '.ini', '.properties', '.cfg', '.json',
    '.yaml', '.yml', '.sql', '.md', '.txt', '.ps1', '.psm1', '.psd1',
    '.cmd', '.bat', '.sh', '.py', '.js', '.jsx', '.ts', '.tsx', '.html',
    '.css', '.scss', '.less', '.vue', '.csv', '.tsv', '.toml', '.gradle',
    '.groovy', '.editorconfig', '.ruleset', '.manifest',
}
SOURCE_NAMES = {
    'jenkinsfile', 'dockerfile', 'makefile', 'start', 'stop', 'restart',
    '.gitignore', '.gitattributes', '.gitmodules', '.editorconfig',
    '.env.example', '.npmrc.example', 'mvnw',
}
GENERATED_DIRECTORIES = {
    '.git', '.vs', '.idea', '__pycache__', 'node_modules', 'runtime',
    'target', 'target', 'target-maven', 'target-stc', 'obj',
    'out', 'logs', '.venv', 'venv',
}
CONFIG_SUFFIXES = {'.ini', '.properties', '.cfg', '.config', '.xml', '.json', '.yaml', '.yml', '.csv', '.tsv'}
MAX_FILE_BYTES = 20 * 1024 * 1024
MAX_TOTAL_BYTES = 200 * 1024 * 1024
SECRET_ASSIGNMENT = re.compile(
    r'''(?im)^\s*["']?[\w.\-]*(?:password|passwd|pwd|secret|token|api[_-]?key)[\w.\-]*["']?\s*[:=]\s*(.*?)\s*$'''
)
XML_SECRET = re.compile(
    r'''(?i)(?:name|key)\s*=\s*["'][^"']*(?:password|passwd|secret|token|api[_-]?key)[^"']*["']\s+value\s*=\s*["']([^"']+)["']'''
)


def git(repository, *arguments, required=True):
    result = subprocess.run(
        ['git', '-C', str(repository), *arguments],
        stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False,
    )
    if required and result.returncode:
        # Do not echo Git stderr: it can contain credential-bearing remote URLs.
        raise RuntimeError('Git command failed: ' + ' '.join(arguments[:2]))
    return result.stdout if result.returncode == 0 else b''


def paths(data):
    return [entry.decode('utf-8') for entry in data.split(b'\0') if entry]


def digest(data):
    return hashlib.sha256(data).hexdigest()


def exclusion(path):
    parts = Path(path).parts
    name = parts[-1].lower()
    if any(part.lower() in GENERATED_DIRECTORIES for part in parts):
        return 'generated/runtime directory'
    if name.startswith('.env') and name != '.env.example':
        return 'local environment/credentials'
    if name.endswith('.local.properties') or name in {
        'apikeylist.csv', 'qtintakeauth.properties', 'dbpoolconfig.ini',
        'dbconfig.ini', '.npmrc', '.pypirc', 'credentials',
    }:
        return 'local connection/credential file'
    if Path(name).suffix not in SOURCE_SUFFIXES and name not in SOURCE_NAMES:
        return 'not a source/configuration/documentation file'
    return None


def literal_secret(value):
    value = value.strip().rstrip(',').strip().strip('\"\'')
    return bool(value) and not (
        value.lower() in {'null', 'none', 'true', 'false', 'changeme', 'change-me'}
        or value.startswith(('${', '$env:', '$ATS_ROOT', '<', '@', '%'))
        or value.endswith('_PASSWORD')
    )


def content_exclusion(path, data):
    if b'\0' in data:
        return 'binary or unsupported text encoding'
    try:
        text = data.decode('utf-8-sig')
    except UnicodeDecodeError:
        return 'non-UTF-8 text; review and transfer separately'
    if '-----BEGIN PRIVATE KEY-----' in text or '-----BEGIN RSA PRIVATE KEY-----' in text:
        return 'private key material'
    if Path(path).suffix.lower() in CONFIG_SUFFIXES:
        assignments = SECRET_ASSIGNMENT.findall(text) + XML_SECRET.findall(text)
        if any(literal_secret(value) for value in assignments):
            return 'possible literal credential in configuration; review separately'
    return None


def collect(repository, package_root, repositories, candidates, excluded):
    relative_root = repository.relative_to(package_root).as_posix()
    if git(repository, 'ls-files', '-u', '-z'):
        raise RuntimeError('Resolve merge conflicts before packaging: ' + relative_root)
    head = git(repository, 'rev-parse', '--verify', 'HEAD', required=False).decode().strip()
    branch = git(repository, 'branch', '--show-current').decode().strip()
    changed = paths(git(repository, 'diff', '--name-only', '--no-renames', '-z', 'HEAD', '--')) if head else paths(git(repository, 'ls-files', '-z'))
    untracked = paths(git(repository, 'ls-files', '--others', '--exclude-standard', '-z'))
    repositories.append({'path': relative_root, 'baseCommit': head or None, 'branch': branch})

    # Submodules and untracked nested repositories must be inspected independently.
    nested = set()
    for entry in git(repository, 'ls-files', '--stage', '-z').split(b'\0'):
        if entry.startswith(b'160000 '):
            nested.add(entry.split(b'\t', 1)[1].decode('utf-8'))
    for name in sorted(set(changed + untracked) | nested):
        absolute = repository / name
        relative = absolute.relative_to(package_root).as_posix().rstrip('/')
        reason = exclusion(relative)
        if absolute.is_symlink():
            excluded.append({'path': relative, 'reason': 'symbolic link'})
            continue
        if not absolute.resolve().is_relative_to(package_root):
            raise RuntimeError('Path escapes package root: ' + relative)
        if absolute.is_dir():
            if any(part.lower() in GENERATED_DIRECTORIES for part in Path(relative).parts):
                excluded.append({'path': relative, 'reason': 'generated/runtime directory'})
                continue
            nested_root = git(absolute, 'rev-parse', '--show-toplevel', required=False).decode('utf-8').strip()
            if nested_root and Path(nested_root).resolve() == absolute.resolve():
                collect(absolute.resolve(), package_root, repositories, candidates, excluded)
            else:
                excluded.append({'path': relative, 'reason': 'directory is not an initialized nested Git repository'})
            continue
        if reason:
            excluded.append({'path': relative, 'reason': reason})
            continue
        original = git(repository, 'rev-parse', '--verify', 'HEAD:' + name, required=False) if head else b''
        candidates.append({
            'path': relative,
            'repository': relative_root,
            'action': 'delete' if not absolute.exists() else ('modify' if original else 'add'),
            'baseBlob': original.decode().strip() or None,
        })


def package(repository, output=None):
    top = git(repository, 'rev-parse', '--show-toplevel').decode('utf-8').strip()
    root = Path(top).resolve()
    if output is None:
        stamp = datetime.now().strftime('%Y%m%d-%H%M%S-%f')
        output = root / 'iteration1/runtime/source-sync' / ('source-changes-' + stamp + '.zip')
    output = Path(output).resolve()
    if output.exists():
        raise RuntimeError('Output already exists; choose a new ZIP filename.')
    repositories, candidates, excluded = [], [], []
    collect(root, root, repositories, candidates, excluded)
    output.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary = tempfile.mkstemp(prefix='source-sync-', suffix='.tmp', dir=output.parent)
    os.close(descriptor)
    entries = []
    total_bytes = 0
    try:
        with zipfile.ZipFile(temporary, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            for candidate in candidates:
                relative = candidate['path']
                if relative.startswith('__source_sync__/'):
                    raise RuntimeError('Reserved package metadata path: ' + relative)
                if candidate['action'] == 'delete':
                    entries.append(candidate)
                    continue
                source = root / relative
                if source.stat().st_size > MAX_FILE_BYTES:
                    raise RuntimeError('Source file exceeds 20 MB: ' + relative)
                data = source.read_bytes()
                reason = content_exclusion(relative, data)
                if reason:
                    excluded.append({'path': relative, 'reason': reason})
                    continue
                total_bytes += len(data)
                if total_bytes > MAX_TOTAL_BYTES:
                    raise RuntimeError('Source package exceeds 200 MB; narrow the repository scope.')
                archive.writestr(relative, data)
                entries.append(dict(candidate, sha256=digest(data), size=len(data)))
            # Detect changes while packaging rather than deliver inconsistent source bytes.
            for entry in entries:
                source = root / entry['path']
                if entry['action'] == 'delete':
                    if source.exists():
                        raise RuntimeError('Deleted file reappeared while packaging: ' + entry['path'])
                elif not source.exists() or digest(source.read_bytes()) != entry['sha256']:
                    raise RuntimeError('File changed while packaging; retry: ' + entry['path'])
            manifest = {
                'formatVersion': 1,
                'createdAtUtc': datetime.now(timezone.utc).isoformat(),
                'scope': 'Current source contents differing from HEAD, including staged and untracked files',
                'repositories': repositories,
                'files': entries,
                'excluded': excluded,
            }
            archive.writestr('__source_sync__/manifest.json', json.dumps(manifest, ensure_ascii=False, indent=2))
            archive.writestr('__source_sync__/README.zh-CN.md', PACKAGE_README)
        os.replace(temporary, output)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)
    return output, manifest


PACKAGE_README = '''# 未提交源码同步包

本包是增量源码，不是完整仓库或可运行程序。包含当前工作区相对 HEAD 的最终文件内容，涵盖暂存、未暂存和未跟踪文件；不保存暂存区的中间版本。

1. 先解压到独立目录，读取 `manifest.json` 的 repositories，核对每个仓库的 baseCommit。嵌套仓库也有各自基线；包内没有 .git，接收端须已有对应仓库。
2. 接收端若有自己的改动，先备份／提交，再比较合并。不要直接覆盖正在协作的工作区。基线不同也要比较合并。
3. 源码按原仓库相对路径放置；`__source_sync__` 是包的元数据，不需要复制进项目。
4. files 中 action=delete 表示源端删除了该文件；仅解压不会删除它。核对目标本地改动后手动处理。重命名表示为删除旧路径、添加新路径。
5. 查看 excluded：忽略文件、编译结果及运行目录不打包；不支持的文件类型、发现的凭据配置会列出排除原因。检测仅为辅助，分享前仍需检查内容，文档／源码可能包含手工写入的信息。
6. 合并后按 parent/common/业务服务顺序构建。包内不含 Maven 依赖和机器配置，不能仅编译 QtIntake 而继续使用旧 Common/QuoteSvr。

文件 SHA-256 对应包内原始字节；baseBlob 是源仓库 HEAD 中的 Git 对象 ID，便于核对基线。本工具不修改 Git 索引、不提交、不推送、不自动应用删除。
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repository', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    output, manifest = package(args.repository, args.output)
    print('ZIP:', output)
    print('Repositories:', len(manifest['repositories']))
    print('Files:', sum(entry['action'] != 'delete' for entry in manifest['files']))
    print('Deletions:', sum(entry['action'] == 'delete' for entry in manifest['files']))
    print('Excluded:', len(manifest['excluded']))
    for entry in manifest['excluded']:
        print('  EXCLUDED:', entry['path'], '-', entry['reason'])
    print('Review __source_sync__/manifest.json before applying the package.')


if __name__ == '__main__':
    try:
        main()
    except (OSError, RuntimeError, UnicodeError) as error:
        print('ERROR:', str(error), file=sys.stderr)
        sys.exit(1)
