@echo off
REM Build script for Yak-130 Aircraft Controller
REM Создание папки для публикации
if not exist "publish" mkdir publish

REM Очистка предыдущей сборки
echo Очистка предыдущей сборки...
dotnet clean -c Release

REM Построение и публикация проекта
echo Постр��ение приложения...
dotnet publish -c Release -o ./publish --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

echo.
echo Готово! Приложение в папке: publish/AircraftController.exe
pause
