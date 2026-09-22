"""Build supplied STC sources locally. Original repositories are read-only inputs."""
from pathlib import Path
import subprocess, shutil, urllib.request, json, hashlib, sys

ROOT = Path(__file__).resolve().parents[1]
BUILD = ROOT / 'runtime' / 'foundation-build'
LIB = BUILD / 'lib'
REPOS = Path('D:/workspace/GitHub')
LEGACY = Path('D:/workspace/greatewall/sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/target/javalib')
JAVA = Path('C:/Program Files/Eclipse Adoptium/jdk-17.0.20.101-hotspot/bin')
LIB.mkdir(parents=True, exist_ok=True)
sys.path.insert(0,str(ROOT/'compat'))
from patch_foundations import patch

def public_jar(group, artifact, version):
    dest = LIB / f'{artifact}-{version}.jar'
    if not dest.exists():
        url = f'https://repo.maven.apache.org/maven2/{group.replace(".","/")}/{artifact}/{version}/{dest.name}'
        urllib.request.urlretrieve(url, dest)
    return dest

def main():
    for src in LEGACY.glob('*.jar'):
        if src.name.startswith(('lombok-', 'tools-', 'jconsole-', 'xml-apis-')): continue
        dest = LIB/src.name
        if not dest.exists(): shutil.copy2(src,dest)
    for src in (ROOT/'cgws-ficc-saletrading-common/lib').glob('*.jar'):
        if src.name.startswith(('gateway-api-', 'lombok-')): continue
        dest=LIB/src.name
        if not dest.exists(): shutil.copy2(src,dest)
    lombok=public_jar('org.projectlombok','lombok','1.18.36')
    for dep in [('com.zaxxer','HikariCP','3.4.5'),('com.alibaba','druid','1.2.27'),
                ('de.schlichtherle.truelicense','truelicense-core','1.33'),
                ('de.schlichtherle.truelicense','truelicense-xml','1.33'),
                ('com.101tec','zkclient','0.10'),('cn.hutool','hutool-all','5.8.29'),
                ('io.netty','netty-handler-proxy','4.1.108.Final'),
                ('io.netty','netty-codec-socks','4.1.108.Final'),
                ('com.thoughtworks.xstream','xstream','1.4.20'),
                ('javax.xml.bind','jaxb-api','2.3.1'),
                ('javax.annotation','javax.annotation-api','1.3.2'),
                ('com.alibaba.csp','sentinel-core','1.8.0')]:
        public_jar(*dep)
    stage=sys.argv[1] if len(sys.argv)>1 else 'dps-api-java'
    if stage == 'gw':
        raise ValueError('Use the standard GW distribution; gateway compilation is not supported')
    original=REPOS/stage/'src/main/java'
    src=BUILD/stage/'src'
    # Prevent obsolete source/classes surviving a branch switch. Delete only generated paths.
    for generated in (src, BUILD/stage/'classes'):
        resolved=generated.resolve()
        if not resolved.is_relative_to(BUILD.resolve()): raise ValueError('Build path outside workspace')
        if resolved.exists(): shutil.rmtree(resolved)
    shutil.copytree(original,src,dirs_exist_ok=True)
    patch(stage,src,ROOT/'compat/patches')
    if stage=='ats-foundation-utilities':
        path=src/'ats/foundation/utils/security/PasswordSecurityHandler.java'
        content=path.read_text(encoding='utf-8')
        content=content.replace('new sun.misc.BASE64Decoder().decodeBuffer(', 'java.util.Base64.getMimeDecoder().decode(')
        content=content.replace('new sun.misc.BASE64Encoder().encode(', 'java.util.Base64.getMimeEncoder(76, new byte[]{10}).encodeToString(')
        path.write_text(content,encoding='utf-8')
    if stage=='ats-app-ym-foundation':
        path=src/'ats/app/ym/utils/encrypt/gx/TripleDes.java'
        content=path.read_text(encoding='utf-8').replace('import sun.misc.BASE64Decoder;', '')
        content=content.replace('BASE64Decoder base64Decoder= new BASE64Decoder();', 'java.util.Base64.Decoder base64Decoder=java.util.Base64.getMimeDecoder();')
        content=content.replace('base64Decoder.decodeBuffer(', 'base64Decoder.decode(')
        content=content.replace('catch (IOException e)', 'catch (IllegalArgumentException e)')
        path.write_text(content,encoding='utf-8')
        path=src/'ats/app/ym/utils/db/resource/BaseStaticDataResource.java'
        content=path.read_text(encoding='utf-8').replace('dbPool.init(dbpoolDefault);','dbPool.init(dbpoolDefault, dbpoolCfg);')
        path.write_text(content,encoding='utf-8')
    out=BUILD/stage/'classes';out.mkdir(parents=True,exist_ok=True)
    jars=sorted((p for p in LIB.glob('*.jar') if p.name not in {stage+'-local.jar'}),key=lambda p: (0 if p.name.endswith('-local.jar') or p.name.startswith(('druid-1.2','hutool-all-5','xstream-1.4','javax.annotation-api-')) else 1,p.name))
    cp=';'.join(str(p) for p in jars)
    sources=list(src.rglob('*.java'))
    if stage=='sc-app-stc':
        # Build the STC messaging/framework module used by QuoteSvr, not unrelated services.
        prefixes=('sc/app/stc/net/','sc/app/stc/fix/','sc/app/stc/po/system/')
        utilities={'Consts.java','TopicUtils.java','ServerWrapper.java'}
        sources=[p for p in sources if p.relative_to(src).as_posix().startswith(prefixes) or
                 (p.parent==src/'sc/app/stc/utils' and p.name in utilities)]
        (BUILD/stage/'selected-sources.txt').write_text('\n'.join(str(p.relative_to(src)) for p in sources),encoding='utf-8')
    args=['-encoding','UTF-8','-cp',cp,'-processorpath',str(lombok),'-d',str(out),'-Xmaxerrs','60']+[str(p) for p in sources]
    argfile=BUILD/stage/'javac.args'
    argfile.write_text('\n'.join('"'+a.replace('\\','/')+'"' for a in args),encoding='utf-8')
    log=BUILD/stage/'compile.log'
    with log.open('w',encoding='utf-8') as f:
        p=subprocess.run([str(JAVA/'javac.exe'),'-J-Duser.language=en','-J-Xmx256m','-J-XX:ActiveProcessorCount=2','@'+str(argfile)],stdout=f,stderr=subprocess.STDOUT,timeout=90)
    print(stage,'exit',p.returncode,'log',log)
    if p.returncode: print(log.read_text(encoding='utf-8',errors='replace')[-18000:]);sys.exit(p.returncode)
    for resource in src.rglob('*'):
        if resource.is_file() and resource.suffix!='.java':
            dest=out/resource.relative_to(src);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(resource,dest)
    output=LIB/(stage+'-local.jar')
    subprocess.run([str(JAVA/'jar.exe'),'cf',str(output),'-C',str(out),'.'],check=True,timeout=30)
    revision=subprocess.check_output(['git','-C',str(REPOS/stage),'rev-parse','HEAD'],text=True).strip()
    (BUILD/stage/'build-info.json').write_text(json.dumps({'source':str(original),'revision':revision,'jar':str(output),'sha256':hashlib.file_digest(output.open('rb'),'sha256').hexdigest()},indent=2),encoding='utf-8')
    print('Built',output)

if __name__=='__main__': main()
