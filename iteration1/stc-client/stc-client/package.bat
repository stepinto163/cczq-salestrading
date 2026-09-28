set EXITCODE=0

for /f %%F in ('dir /b /a:d control') do call:pack %%F

if ERRORLEVEL 1 goto failed
goto:end

:pack
set env=%~1
set name=%AppName%-%env%-%Version%
set exists_flag=false
set isGSEnv=false
set isGREnv=false
set isIDBProEnv=false
set exeName=%AppExe%
cd %WORKSPACE%
echo %env%|find "DEV">nul && set exists_flag=true
if "%exists_flag%"=="false" (
	echo %env%|find "POC">nul && set exists_flag=true
) else (
	echo %env%|find "DEV">nul && set exeName=%AppExe%-%env%
)
if "%exists_flag%"=="false" (
	echo %env%|find "QA">nul && set exists_flag=true
) else (
	echo %env%|find "POC">nul && set exeName=%AppExe%-%env%
)
if "%exists_flag%"=="false" (
	echo %env%|find "SIT">nul && set exists_flag=true
	
	if "%exists_flag%"=="true" (
		echo %env%|find "SIT">nul && set exeName=%AppExe%-%env%
	)
	
)
if "%exists_flag%"=="false" (
	echo %env%|find "CITY">nul && set exists_flag=true
	
	if "%exists_flag%"=="true" (
		echo %env%|find "CITY">nul && set exeName=%AppExe%-%env%
	)	
)
if "%exists_flag%"=="false" (
	echo %env%|find "POC2">nul && set exists_flag=true
	
	if "%exists_flag%"=="true" (
		echo %env%|find "POC2">nul && set exeName=%AppExe%-%env%
	)	
)

if "%exists_flag%"=="false" (
	echo %env%|find "UAT">nul && set exists_flag=true
	
	if "%exists_flag%"=="true" (
		echo %env%|find "UAT">nul && set exeName=%AppExe%-%env%
	)
	
) else (
	echo %env%|find "QA">nul && set exeName=%AppExe%-%env%
)

echo %env%|find "IDBPro">nul && set isIDBProEnv=true
if "%isIDBProEnv%"=="true" (
	set exeName=%AppExe%
)
echo %env%|find "IDBPro">nul && set isGREnv=true

if "%exists_flag%"=="true" (
	copy control\%env%\%AppExe%.exe.config client\ /Y
	copy control\%env%\dps.exe.config client\ /Y
	copy client\STC.exe client\STC-%env%.exe /Y
	copy control\%env%\%AppExe%.exe.config client\%AppExe%-%env%.exe.config /Y
	copy client\dps.exe client\dps-%env%.exe /Y
	copy control\%env%\dps.exe.config client\dps-%env%.exe.config /Y
  copy control\%env%\STCExcel-AddIn64-packed.xll.config client\excelAddin\ /Y
  copy control\%env%\STCExcel-AddIn-packed.xll.config client\excelAddin\ /Y
  copy client\QQHelper.exe client\ /Y
  copy control\%env%\QQHelper.exe.config client\ /Y
) else (
	copy control\%env%\%AppExe%.exe.config client\ /Y
	copy control\%env%\dps.exe.config client\ /Y
    copy control\%env%\STCExcel-AddIn64-packed.xll.config client\excelAddin\ /Y
    copy control\%env%\STCExcel-AddIn-packed.xll.config client\excelAddin\ /Y
)
cd %WORKSPACE%
rmdir /S /Q %AppName%
xcopy client %AppName% /I /E
cd client
call %WORKSPACE%\control\%env%\update.bat
cd %WORKSPACE%
copy client\autoUpdaterUrl.txt %AppName%\ /Y
copy client\AutoUpdateService.xml %AppName%\ /Y
copy client\control\autoupdater.config %AppName%\control\ /Y
copy client\control\filedetail.config %AppName%\control\ /Y
copy client\control\filedetailservice.xml %AppName%\control\ /Y
echo %env%|find "GR">nul && set isGREnv=true
if "%isGREnv%"=="false" (
copy D:\jenkins-slave\workspace\STC-lib\libcef.dll %AppName%\bin\ /Y
)
del %AppName%\bin\YamlDotNet.dll
7z a %name%.zip %AppName% -r -x!*.pdb

echo %env%|find "GS">nul && set isGSEnv=true
python apply_configuration.py -i STC.Setup/salt-formula/stc.setup/files/ -o ./STC.Setup/ -c STC.Setup/salt-formula/config.yaml -n idc.setup -e %env%
if "%isGSEnv%"=="true" (
	"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=SingleImage STC.Setup\STC.Setup.sln
	copy "STC.Setup\STC.Setup\Express\SingleImage\DiskImages\DISK1\IDC Broker.exe" %name%.exe
)
goto:eof

:failed
set EXITCODE=1
:end
EXIT /B %EXITCODE%