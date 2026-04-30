@echo off
REM Installer script for Yak-130 Aircraft Controller
REM Creates a simple MSI installer

echo Создание инсталлятора...
echo.

REM Проверка наличия WiX Toolset
where candle >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ОШИБКА: WiX Toolset не установлен!
    echo Скачайте с https://wixtoolset.org/
    pause
    exit /b 1
)

REM Создание папки для инсталлятора
if not exist "installer" mkdir installer
if not exist "installer\bin" mkdir installer\bin

REM Копирование .exe файла
echo Копирование приложения...
if not exist "publish\AircraftController.exe" (
    echo ОШИБКА: publish\AircraftController.exe не найден!
    echo Сначала запустите build.bat
    pause
    exit /b 1
)

xcopy /Y /Q "publish\*.*" "installer\bin\"

REM Создание WiX файла
echo Создание конфигурации инсталлятора...
(
    @echo ^<?xml version="1.0" encoding="UTF-8"?^>
    @echo ^<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi"^>
    @echo   ^<Product Id="*" Name="Як-130 Контроллер" Language="1049" Version="1.0.0.0"
    @echo     Manufacturer="Limfouzel1"^>
    @echo     ^<Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" /^>
    @echo     ^<MajorUpgrade DowngradeErrorMessage="Более новая версия уже установлена." /^>
    @echo     ^<Media Id="1" Cabinet="AircraftController.cab" EmbedCab="yes" /^>
    @echo.
    @echo     ^<Feature Id="ProductFeature" Title="Як-130 Контроллер" Level="1"^>
    @echo       ^<ComponentRef Id="AircraftControllerExe" /^>
    @echo     ^</Feature^>
    @echo   ^</Product^>
    @echo.
    @echo   ^<Fragment^>
    @echo     ^<Directory Id="TARGETDIR" Name="SourceDir"^>
    @echo       ^<Directory Id="ProgramFilesFolder"^>
    @echo         ^<Directory Id="INSTALLFOLDER" Name="AircraftController" /^>
    @echo       ^</Directory^>
    @echo     ^</Directory^>
    @echo   ^</Fragment^>
    @echo.
    @echo   ^<Fragment^>
    @echo     ^<ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER"^>
    @echo       ^<Component Id="AircraftControllerExe"^>
    @echo         ^<File Id="AircraftControllerEXE" Source="bin\AircraftController.exe" /^>
    @echo       ^</Component^>
    @echo     ^</ComponentGroup^>
    @echo   ^</Fragment^>
    @echo ^</Wix^>
) > "installer\Product.wxs"

REM Компиляция WiX файла
echo Компиляция...
candle -o "installer\obj\" "installer\Product.wxs"

if %ERRORLEVEL% NEQ 0 (
    echo ОШИБКА при компиляции!
    pause
    exit /b 1
)

REM Связывание (линковка)
echo Создание MSI...
light -out "AircraftController-Installer.msi" "installer\obj\Product.wixobj"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✓ Инсталлятор создан: AircraftController-Installer.msi
) else (
    echo ОШИБКА при создании инсталлятора!
)

pause
