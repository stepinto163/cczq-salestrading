param([ValidateSet('all','common','quotesvr','notificationsvr')][string]$Module='all')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$foundation= & (Join-Path $PSScriptRoot 'foundation-classpath.ps1')
$commonJar=Join-Path $taskRoot 'cczq-app-salestrading-common/target/cczq-app-salestrading-common.jar'
# Rebuilding common changes both processes' runtime dependency. Stop them first.
foreach($service in @('quotesvr','notificationsvr')){
    $pidFile=Join-Path $taskRoot "runtime/$service.pid"
    if(Test-Path -LiteralPath $pidFile){
        $serviceId=[int](Get-Content -LiteralPath $pidFile)
        if(Get-Process -Id $serviceId -ErrorAction SilentlyContinue){throw "Stop $service before rebuilding the shared library."}
    }
}
$modules=@('common')
if($Module -eq 'all'){$modules+=@('quotesvr','notificationsvr')}elseif($Module -ne 'common'){$modules+=$Module}
foreach($name in $modules){
    $moduleRoot=Join-Path $taskRoot "cczq-app-salestrading-$name"
    $target=[IO.Path]::GetFullPath((Join-Path $moduleRoot 'target'))
    if(!$target.StartsWith($taskRoot+[IO.Path]::DirectorySeparatorChar)){throw 'Build target outside project'}
    if(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target -Recurse -Force}
    $classes=Join-Path $target 'classes'
    New-Item -ItemType Directory -Force -Path $classes | Out-Null
    $sourcePaths=@(Get-ChildItem (Join-Path $moduleRoot 'src/main/java') -Filter '*.java' -Recurse | Select-Object -ExpandProperty FullName)
    $classpath=$foundation
    if($name -ne 'common'){$classpath=$commonJar+';'+$foundation}
    & javac '-J-Xmx192m' '-J-XX:ActiveProcessorCount=2' '-J-Duser.language=en' -proc:none -encoding UTF-8 -cp $classpath -d $classes @sourcePaths
    if($LASTEXITCODE -ne 0){throw "$name compile failed"}
    $jarPath=Join-Path $target "cczq-app-salestrading-$name.jar"
    & jar --create --file $jarPath -C $classes .
    if($LASTEXITCODE -ne 0){throw "$name packaging failed"}
    Write-Output "Built $jarPath"
}
