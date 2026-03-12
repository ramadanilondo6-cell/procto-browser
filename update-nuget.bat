@echo off
echo Mengupdate NuGet packages untuk Procto...
echo.

cd /d "%~dp0Procto"

echo Updating CefSharp.Common.NETCore...
dotnet add package CefSharp.Common.NETCore

echo Updating CefSharp.Wpf.NETCore...
dotnet add package CefSharp.Wpf.NETCore

echo Updating Serilog...
dotnet add package Serilog

echo Updating Serilog.Sinks.File...
dotnet add package Serilog.Sinks.File

echo Updating Newtonsoft.Json...
dotnet add package Newtonsoft.Json

echo.
echo Selesai mengupdate packages.
pause
