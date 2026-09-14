$taskRoot=Split-Path $PSScriptRoot -Parent
$lib=Join-Path $taskRoot 'runtime/foundation-build/lib'
$mavenClasspath=Join-Path $taskRoot 'runtime/foundation-classpath.txt'
if(!(Test-Path -LiteralPath $lib)){
    # Frozen JAR set absent: fall back to the classpath resolved from the internal Maven repository.
    # Regenerate with scripts/build-foundations.ps1 (mvn dependency:build-classpath on stc-foundation-runtime).
    if(!(Test-Path -LiteralPath $mavenClasspath)){
        throw "Missing $lib and $mavenClasspath. Run scripts/build-foundations.ps1 first."
    }
    (Get-Content -LiteralPath $mavenClasspath -Raw).Trim()
}else{
    $jars=Get-ChildItem -LiteralPath $lib -Filter '*.jar' | Where-Object Name -ne 'gw-local.jar' | Sort-Object @{Expression={if($_.Name -like '*-local.jar'){0}elseif($_.Name -match '^(druid-1.2|hutool-all-5|xstream-1.4|javax.annotation-api-)'){1}else{2}}},Name
    ($jars.FullName -join ';')
}
