@echo off
setlocal EnableDelayedExpansion
title Build Procto Config Editor

echo.
echo  =============================================
echo    PROCTO CONFIG EDITOR - Build Script
echo  =============================================
echo.

set TARGET_RID=win-x64
set OUT_DIR=%~dp0publish\Editor

echo [1/3] Membersihkan folder output sebelumnya...
if exist "%OUT_DIR%" rmdir /s /q "%OUT_DIR%"

echo [2/3] Menjalankan dotnet publish...
echo       Mohon tunggu...
echo.

dotnet publish "%~dp0ProctoConfigEditor\ProctoConfigEditor.csproj" -c Release -r %TARGET_RID% --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_DIR%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  [ERROR] Build GAGAL!
    echo.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo  =============================================
echo   [SUKSES] Build berhasil!
echo   Output : %OUT_DIR%
echo   Jalankan: ProctoConfigEditor.exe
echo  =============================================
echo.
pause
endlocal