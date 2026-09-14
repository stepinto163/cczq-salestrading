$taskRoot=Split-Path $PSScriptRoot -Parent
$lib=Join-Path $taskRoot 'runtime/foundation-build/lib'
$jars=Get-ChildItem -LiteralPath $lib -Filter '*.jar' | Where-Object Name -ne 'gw-local.jar' | Sort-Object @{Expression={if($_.Name -like '*-local.jar'){0}elseif($_.Name -match '^(druid-1.2|hutool-all-5|xstream-1.4|javax.annotation-api-)'){1}else{2}}},Name
($jars.FullName -join ';')
