@echo off
setlocal EnableDelayedExpansion
title Procto Build Tool

echo.
echo  =============================================
echo    PROCTO  -  Build Script v2.0
echo  =============================================
echo.

echo [*] Menutup proses Procto yang sedang berjalan...
taskkill /f /im Procto.exe /t >nul 2>&1
timeout /t 1 /nobreak >nul

:menu_arch
echo.
echo  Pilih Arsitektur:
echo    1.  x64 - 64-bit  [Rekomendasi]
echo    2.  x86 - 32-bit
echo.
set /p ARCH_CHOICE="  Pilihan [1/2, Default=1]: "
if "%ARCH_CHOICE%"=="" set ARCH_CHOICE=1
if "%ARCH_CHOICE%"=="1" goto arch_x64
if "%ARCH_CHOICE%"=="2" goto arch_x86
echo  [!] Pilihan tidak valid.
goto menu_arch

:arch_x64
set TARGET_RID=win-x64
set ARCH_LABEL=x64 64-bit
goto menu_single

:arch_x86
set TARGET_RID=win-x86
set ARCH_LABEL=x86 32-bit
goto menu_single

:menu_single
echo.
echo  Pilih Format Output:
echo    1.  Single EXE  - Satu file Procto.exe  [Rekomendasi]
echo    2.  Standard    - Banyak file DLL dalam folder
echo.
set /p SINGLE_CHOICE="  Pilihan [1/2, Default=1]: "
if "%SINGLE_CHOICE%"=="" set SINGLE_CHOICE=1
if "%SINGLE_CHOICE%"=="1" goto single_yes
if "%SINGLE_CHOICE%"=="2" goto single_no
echo  [!] Pilihan tidak valid.
goto menu_single

:single_yes
set PUBLISH_SINGLE=true
set MODE_LABEL=Single File EXE
goto start_build

:single_no
set PUBLISH_SINGLE=false
set MODE_LABEL=Standard multi-file
goto start_build

:start_build
set LOG_FILE=%~dp0build_custom.log
set OUT_DIR=%~dp0publish\%TARGET_RID%

echo.
echo  =============================================
echo   Konfigurasi Build:
echo     Arsitektur : %ARCH_LABEL%
echo     Mode       : %MODE_LABEL%
echo     Output     : %OUT_DIR%
echo  =============================================
echo.

echo Build Procto dimulai pada %DATE% %TIME% > "%LOG_FILE%"
echo Arsitektur : %TARGET_RID% >> "%LOG_FILE%"
echo Mode       : %MODE_LABEL% >> "%LOG_FILE%"
echo. >> "%LOG_FILE%"

echo [1/4] Membersihkan folder output sebelumnya...
if exist "%OUT_DIR%" rmdir /s /q "%OUT_DIR%" >> "%LOG_FILE%" 2>&1

echo [2/4] Menjalankan dotnet publish...
echo       Mohon tunggu 1-3 menit...
echo.

dotnet publish "%~dp0SafeExamCEF\SafeExamCEF.csproj" -c Release -r %TARGET_RID% --self-contained true -p:PublishSingleFile=%PUBLISH_SINGLE% -p:IncludeNativeLibrariesForSelfExtract=true -p:IncludeAllContentForSelfExtract=true -o "%OUT_DIR%" >> "%LOG_FILE%" 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  [ERROR] Build GAGAL! Periksa log: %LOG_FILE%
    echo.
    pause
    exit /b %ERRORLEVEL%
)

echo [3/4] Menyalin file konfigurasi...
if exist "%~dp0default.safeexam.json" (
    if not exist "%OUT_DIR%\config" mkdir "%OUT_DIR%\config"
    copy /Y "%~dp0default.safeexam.json" "%OUT_DIR%\config\default.safeexam.json" >> "%LOG_FILE%" 2>&1
    echo       Konfigurasi disalin.
) else (
    echo       Konfigurasi tidak ditemukan, dilewati.
)

echo [4/4] Finalisasi...
if exist "%OUT_DIR%\Procto.exe" (
    for %%F in ("%OUT_DIR%\Procto.exe") do (
        set /a SIZEMB=%%~zF / 1048576
        echo       Procto.exe ukuran: !SIZEMB! MB
    )
)

echo Build selesai: %DATE% %TIME% >> "%LOG_FILE%"
echo.
echo  =============================================
echo   [SUKSES] Build berhasil!
echo   Output : %OUT_DIR%
echo   Jalankan: Procto.exe
echo  =============================================
echo.
pause
endlocal