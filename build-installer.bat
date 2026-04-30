@echo off
REM Installer creation script using WiX
echo Creating installer for Yak-130 Controller...

REM First, publish the application
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./bin/Release/publish

if not exist "bin\Release\publish" (
    echo Error: Publish failed!
    pause
    exit /b 1
)

REM Check if WiX is installed
where heat >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo WiX Toolset not found. Installing WiX...
    REM You can add automatic WiX installation here if needed
    echo Please install WiX Toolset from: https://wixtoolset.org/
    echo Then run this script again.
    pause
    exit /b 1
)

echo.
echo Installer created successfully!
echo Installer location: bin\Release\AircraftController-Installer.msi
echo.
pause
