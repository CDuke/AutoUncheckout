@echo off
cls
IF NOT EXIST "tools/FAKE" (
	"tools/nuget/nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
)

"tools/FAKE/tools/Fake.exe" "%~dp0build/build.fsx" "VisualStudioVersion=11.0"
pause
exit /b %errorlevel%