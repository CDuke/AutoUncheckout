@echo off
cls
IF NOT EXIST "tools/FAKE" (
	"tools/nuget/nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
)

"tools/FAKE/tools/Fake.exe" "build.fsx"
pause
exit /b %errorlevel%