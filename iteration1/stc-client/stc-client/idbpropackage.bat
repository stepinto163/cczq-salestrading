set EXITCODE=0

for /f %%F in ('dir /b /a:d control') do call:pack %%F

if ERRORLEVEL 1 goto failed
goto:end

:pack
set env=%~1
set name=%env%-%Version%
set isIDBProEnv=false
set exeName=%AppExe%
cd %WORKSPACE%

echo %env%|find "IDBPro">nul && set isIDBProEnv=true
if "%isIDBProEnv%"=="true" (
	copy control\%env%\%AppExe%.exe.config client\ /Y
	copy control\%env%\dps.exe.config client\ /Y

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

	del %AppName%\bin\YamlDotNet.dll
	7z a %name%.zip %AppName% -r -x!*.pdb

)
goto:eof

:failed
set EXITCODE=0
:end
EXIT /B %EXITCODE%