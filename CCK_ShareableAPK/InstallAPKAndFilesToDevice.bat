@echo off
setlocal enabledelayedexpansion

REM Define the package name of the app

REM Set the source folder and destination path
set "source_folder=%~dp0ShareableAPK\"
set "apk_folder=%~dp0ShareableAPK\"


REM Specify the paths to adb and aapt in the script's folder
set "adb_path=%~dp0tools\adb.exe"
set "aapt_path=%~dp0tools\aapt.exe"

REM Check if the Android device is connected
"%adb_path%" devices >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Android device is not connected.
    pause
    exit /b
)

echo Searching for APK files in the current directory...
for %%i in ("%apk_folder%*.apk") do (
    set "apk=%%i"
    echo Found APK: !apk!
    echo Installing !apk!...
    "%adb_path%" install "!apk!"
    echo Installation of !apk! complete.

    REM Use aapt to extract the package name
    REM Use aapt to dump the information to a temporary file
    "%aapt_path%" dump badging "!apk!" > temp.txt

    REM Extract the package name from the temporary file
    for /f "tokens=2 delims=: " %%p in ('findstr /c:"package: name=" temp.txt') do (
        set "PACKAGE_NAME=%%p"
    )

rem Remove unwanted characters from the package name
set "PACKAGE_NAME=!PACKAGE_NAME:~6,-1!"


    REM Check if the package name is empty
    if "!PACKAGE_NAME!" == "" (
        echo Error: Package name could not be extracted from !apk!
        pause
        exit /b
    )

    echo Package name: !PACKAGE_NAME!
    del temp.txt
)

set "android_destination=sdcard/Android/data/%PACKAGE_NAME%/files/"

REM Check if the destination folder exists on the Android device
"%adb_path%" shell mkdir -p "!android_destination!"

REM Loop through all video files in the current directory
for %%F in ("%source_folder%*.mp4" "%source_folder%*.mov" "%source_folder%*.avi" "%source_folder%*.mkv") do (
    REM Construct the destination path on the Android device
    set "destination_path=!android_destination!%%~nxF"
    
	echo Destination Path "!destination_path!"
IF EXIST "!destination_path!" (
    echo File exists at the specified path.
    REM Add your code here to handle the case when the file exists.
) else (
    echo File does not exist at the specified path.
    REM Add your code here to handle the case when the file does not exist.
		"%adb_path%" push "%%F" "!destination_path!"
    
		REM Check if the copy operation was successful
		if %errorlevel% neq 0 (
			echo Failed to copy "%%~nxF" to "!destination_path!".
		) else (
			echo Copied "%%~nxF" to "!destination_path!" successfully.
		)
)

)

endlocal

REM Pause to see the output
pause