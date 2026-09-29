param(
    [Parameter(Mandatory=$true)][ValidateSet('baseline', 'typed')][string]$Mode
)
# Run only after explicit authorization. Fixed 1,000-row input, no UI, no load escalation.
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$java = Join-Path $root 'iteration1/runtime/tools/jdk8/jdk8u504-b01/bin/java.exe'
$module = Join-Path $root 'iteration1/cgws-ficc-saletrading-quotesvr'
$output = Join-Path $root "iteration1/runtime/protocol-cost-$Mode.json"
$stdout = "$output.stdout.log"
$stderr = "$output.stderr.log"
$existing = Get-CimInstance Win32_Process -Filter "Name='java.exe'" |
    Where-Object { $_.CommandLine -match 'ficc.quote.ProtocolCostProbe' }
if ($existing) { throw 'A protocol cost probe is already running.' }
$common = Join-Path $root 'iteration1/cgws-ficc-saletrading-common/target/classes'
$classpath = "$common;$module/target/test-classes;$module/target/classes;$module/target/javalib/*"
$arguments = @('-Xms32m', '-Xmx128m', '-XX:ActiveProcessorCount=2', '-cp',
    "`"$classpath`"", 'ficc.quote.ProtocolCostProbe', $Mode, "`"$output`"")
$process = Start-Process -FilePath $java -ArgumentList $arguments -WorkingDirectory $root `
    -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Protocol cost probe exceeded its 15-second watchdog; no further run started.'
}
$process.WaitForExit()
if ($process.ExitCode -ne 0) {
    throw "Protocol cost probe failed; see $stderr"
}
Get-Content -LiteralPath $output -Encoding UTF8
