set EXITCODE=0

for /f %%F in ('dir /b /a:d controlbdb') do call:pack %%F

if ERRORLEVEL 1 goto failed
goto:end

:pack
set env=%~1
set name=%AppName%-%env%-%Version%
set isBDBEnv=false
set exeName=%AppExe%
cd %WORKSPACE%
	copy controlbdb\%env%\%AppExe%.exe.config client\ /Y
	copy controlbdb\%env%\dps-bdb.exe.config client\ /Y
       copy controlbdb\%env%\STCExcel-AddIn64-packed.xll.config client\excelAddin\ /Y
       copy controlbdb\%env%\STCExcel-AddIn-packed.xll.config client\excelAddin\ /Y
       copy client\dps.exe client\dps-%env%.exe /Y
       copy client\dps.exe client\dps-bdb.exe /Y
       copy client\BDB.exe.config client\IDCClient.exe.config /Y
       copy client\BDB.exe client\IDCClient.exe /Y
       copy D:\jenkins-slave\workspace\STC-lib\libcef.dll client\bin\ /Y
       copy client\plugin\STC.Chrome.Plugin.dll client\plugin\BDB.Chrome.Plugin.dll /Y
       copy client\plugin\STC.Data.Plugin.dll client\plugin\BDB.Data.Plugin.dll /Y
       copy client\plugin\STC.dps.plugin.dll client\plugin\BDB.dps.plugin.dll /Y
       copy client\plugin\STC.Excel.Plugin.dll client\plugin\BDB.Excel.Plugin.dll /Y
       copy client\plugin\STC.InnerBoard.Plugin.dll client\plugin\BDB.InnerBoard.Plugin.dll /Y
       copy client\plugin\STC.Sys.Plugin.dll client\plugin\BDB.Sys.Plugin.dll /Y
       copy client\plugin\STC.XBond.Plugin.dll client\plugin\BDB.XBond.Plugin.dll /Y
       copy client\plugin\STC.WorkBench.Plugin.dll client\plugin\BDB.WorkBench.Plugin.dll /Y
       copy client\plugin\STC.quant.plugin.dll client\plugin\BDB.quant.plugin.dll /Y
       cd %WORKSPACE%
	rmdir /S /Q %AppName%
	xcopy client %AppName% /I /E
	cd client
	call %WORKSPACE%\controlbdb\%env%\update.bat
	cd %WORKSPACE%
	copy client\autoUpdaterUrl.txt %AppName%\ /Y
	copy client\AutoUpdateService.xml %AppName%\ /Y
	copy client\control\autoupdater.config %AppName%\control\ /Y
	copy client\control\filedetail.config %AppName%\control\ /Y
	copy client\control\filedetailservice.xml %AppName%\control\ /Y
	del %AppName%\bin\YamlDotNet.dll
	del %AppName%\bin\libcef.zip.001
	del %AppName%\bin\libcef.zip.002
	del %AppName%\STC.exe.config
       del STC.Frame\UI\BDB\*.* /Q
       rmdir STC.Frame\UI\BDB /S/Q
       rmdir STC.Frame\UI /S/Q
       rmdir STC.Frame /S/Q
       del STC.Common\Resources\BDB\*.* /Q
       rmdir STC.Common\Resources\BDB /S/Q
       rmdir STC.Common\Resources/S/Q
       rmdir STC.Common /S/Q
       7z a %name%.zip %AppName% -r -x!*.pdb
       python apply_configuration.py -i STC.Setup/salt-formula/StcLite.setup/files/ -o ./StcLite.Setup/ -c STC.Setup/salt-formula/configbdb.yaml -n idc.setup -e %env%
goto:eof

:failed
set EXITCODE=1
:end
EXIT /B %EXITCODE%