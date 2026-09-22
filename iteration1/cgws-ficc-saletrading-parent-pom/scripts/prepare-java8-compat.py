"""Prepare local Java 8 compatibility dependencies without modifying STC repositories."""
from pathlib import Path
import argparse
import subprocess
import shutil
import hashlib
import xml.etree.ElementTree as E

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--foundation-build', type=Path, required=True,
                    help='Existing foundation-build directory with src/classes/javac.args')
parser.add_argument('--runtime-pom', type=Path, required=True,
                    help='Original stc-foundation-runtime.pom.xml')
parser.add_argument('--jdk17', type=Path, required=True,
                    help='JDK 17 used only to rebuild staged dependencies with --release 8')
parser.add_argument('--output', type=Path, required=True)
parser.add_argument('--maven', type=Path, required=True)
options = parser.parse_args()
original = options.foundation_build.resolve()
output = options.output.resolve()
java = options.jdk17.resolve() / 'bin'
if output == original or original in output.parents or output in original.parents:
    raise SystemExit('Output must be separate from the original dependency directory')
if output.exists() and any(output.iterdir()):
    raise SystemExit('Choose a fresh empty output directory to avoid stale classes')
output.mkdir(parents=True, exist_ok=True)
for stage in ['dps-api-java','ats-foundation-utilities','ats-app-ym-foundation','ats-bbox-framework','sc-app-common','sc-app-stc']:
 stageOut=output/stage;stageOut.mkdir(exist_ok=True);classes=stageOut/'classes';classes.mkdir(exist_ok=True)
 args=[line.strip().strip('"') for line in (original/stage/'javac.args').read_text(encoding='utf-8').splitlines()]
 args[args.index('-d')+1]=str(classes)
 args=['--release','8']+args
 argfile=stageOut/'javac.args';argfile.write_text('\n'.join('"'+x.replace('\\','/')+'"' for x in args),encoding='utf-8')
 with (stageOut/'compile.log').open('w',encoding='utf-8') as f:
  result=subprocess.run([str(java/'javac.exe'),'-J-Xmx256m','-J-XX:ActiveProcessorCount=2','@'+str(argfile)],stdout=f,stderr=subprocess.STDOUT,timeout=90)
 print(stage,result.returncode,flush=True)
 if result.returncode: print((stageOut/'compile.log').read_text(encoding='utf-8',errors='replace')[-5500:]);raise SystemExit(result.returncode)
 for resource in (original/stage/'classes').rglob('*'):
  if resource.is_file() and resource.suffix!='.class':
   dest=classes/resource.relative_to(original/stage/'classes');dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(resource,dest)
 subprocess.run([str(java/'jar.exe'),'cf',str(output/(stage+'-local.jar')),'-C',str(classes),'.'],check=True,timeout=20)

ns='http://maven.apache.org/POM/4.0.0';E.register_namespace('',ns)
def el(p,n,t=None):
 x=E.SubElement(p,'{'+ns+'}'+n)
 if t is not None:x.text=str(t)
 return x
def pom(artifact,version,packaging='jar'):
 p=E.Element('{'+ns+'}project');el(p,'modelVersion','4.0.0');el(p,'groupId','cgws.ficc.saletrading.compat');el(p,'artifactId',artifact);el(p,'version',version);el(p,'packaging',packaging);return p
def write(p,file):
 E.indent(p,space='    ');E.ElementTree(p).write(file,encoding='utf-8',xml_declaration=True)
runtime=E.parse(options.runtime_pom).getroot()
for field,value in [('groupId','cgws.ficc.saletrading.compat'),('artifactId','stc-foundation-runtime-java8'),('version','1.0-SNAPSHOT')]:runtime.find('{'+ns+'}'+field).text=value
bootstrap=pom('java8-local-install','1.0-SNAPSHOT','pom');plugin=el(el(el(bootstrap,'build'),'plugins'),'plugin');el(plugin,'groupId','org.apache.maven.plugins');el(plugin,'artifactId','maven-install-plugin');el(plugin,'version','3.1.3');executions=el(plugin,'executions')
def install(artifact,file,pomfile):
 ex=el(executions,'execution');el(ex,'id',artifact);el(ex,'phase','validate');el(el(ex,'goals'),'goal','install-file');c=el(ex,'configuration');el(c,'file',file);el(c,'pomFile',pomfile)
for d in runtime.find('{'+ns+'}dependencies'):
 artifact=d.find('{'+ns+'}artifactId').text
 if artifact.endswith('-local'):
  file=output/(artifact+'.jar');version='java8-'+hashlib.sha256(file.read_bytes()).hexdigest()[:12]
  d.find('{'+ns+'}groupId').text='cgws.ficc.saletrading.compat';d.find('{'+ns+'}version').text=version
  p=output/(artifact+'.pom');write(pom(artifact,version),p);install(artifact,file,p)
p=output/'runtime.pom';write(runtime,p);install('runtime',p,p);write(bootstrap,output/'install-pom.xml')

subprocess.run([str(options.maven.resolve()), "-o", "-B", "-ntp", "-f", str(output / "install-pom.xml"), "validate"], check=True, timeout=120)
