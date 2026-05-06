@echo off
setlocal

if "%UNITY_PATH_6000_3_10%"=="" (
    echo ERROR: UNITY_PATH_6000_3_10 environment variable is not set.
    echo Example: set UNITY_PATH_6000_3_10=C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe
    exit /b 1
)

if not exist "%UNITY_PATH_6000_3_10%" (
    echo ERROR: UNITY_PATH_6000_3_10 does not point to a valid file: "%UNITY_PATH_6000_3_10%"
    exit /b 1
)

where 7z >nul 2>&1
if errorlevel 1 (
    echo ERROR: 7z was not found in PATH.
    exit /b 1
)

where butler >nul 2>&1
if errorlevel 1 (
    echo ERROR: butler was not found in PATH.
    exit /b 1
)

set "ITCH_CHANNEL=grisha-gu/dice-miner:html5"
set "PROJECT_NAME=DiceMiner"

set "ITCH_VERSION=%~2"

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "PROJECT_PATH=%%~fI"
set "BUILD_DIR=%PROJECT_PATH%\Build\WebGL\%PROJECT_NAME%"
set "ARCHIVE_PATH=%PROJECT_PATH%\Build\WebGL\%PROJECT_NAME%.zip"
set "LOG_PATH=%PROJECT_PATH%\Build\WebGL\%PROJECT_NAME%_build.log"

echo.
echo [1/4] Building WebGL via Unity...
"%UNITY_PATH_6000_3_10%" -batchmode -quit -nographics -projectPath "%PROJECT_PATH%" -executeMethod GrishaWorkshops.Tools.BuildTool.BuildFromCommandLine -buildOutput "%BUILD_DIR%" -logFile "%LOG_PATH%"
if errorlevel 1 (
    echo ERROR: Unity build failed. Check log: "%LOG_PATH%"
    exit /b 1
)

echo.
echo [2/4] Compressing build with 7z...
if exist "%ARCHIVE_PATH%" del /f /q "%ARCHIVE_PATH%"
7z a -tzip "%ARCHIVE_PATH%" "%BUILD_DIR%\*"
if errorlevel 1 (
    echo ERROR: 7z compression failed.
    exit /b 1
)

echo.
echo [3/4] Uploading archive to itch.io with butler...
if "%ITCH_VERSION%"=="" (
    butler push "%ARCHIVE_PATH%" "%ITCH_CHANNEL%"
) else (
    butler push "%ARCHIVE_PATH%" "%ITCH_CHANNEL%" --userversion "%ITCH_VERSION%"
)
if errorlevel 1 (
    echo ERROR: Butler upload failed.
    exit /b 1
)

echo.
echo [4/4] Cleaning Build\WebGL folder...
if exist "%PROJECT_PATH%\Build\WebGL" (
    for /d %%D in ("%PROJECT_PATH%\Build\WebGL\*") do rmdir /s /q "%%~fD"
    del /f /q "%PROJECT_PATH%\Build\WebGL\*" >nul 2>&1
)

echo.
echo Done.
echo Uploaded: "%ARCHIVE_PATH%" -^> "%ITCH_CHANNEL%"
exit /b 0
