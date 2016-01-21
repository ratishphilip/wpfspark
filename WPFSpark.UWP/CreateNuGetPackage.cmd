@REM This script must be executed inside a Developer Command Prompt for Visual Studio
@REM Run this from the Solution Directory

@echo.
@echo ===== Building WPFSpark.UWP.dll (x86) =====
@echo.
@msbuild WPFSpark.UWP\WPFSpark.UWP.csproj /p:Configuration="Release" /p:Platform="x86"

@echo.
@echo ===== Building WPFSpark.UWP.dll (x64) =====
@echo.
@msbuild WPFSpark.UWP\WPFSpark.UWP.csproj /p:Configuration="Release" /p:Platform="x64"

@echo.
@echo ===== Building WPFSpark.UWP.dll (ARM) =====
@echo.
@msbuild WPFSpark.UWP\WPFSpark.UWP.csproj /p:Configuration="Release" /p:Platform="ARM"

@echo.
@echo ===== Updating Reference file =====
@copy /Y WPFSpark.UWP\bin\x86\Release\WPFSpark.UWP.cs WPFSpark.UWP.Ref

@echo.
@echo ===== Building WPFSpark.UWP.dll (reference) =====
@echo.
@msbuild WPFSpark.UWP.Ref\WPFSpark.UWP.Ref.csproj /p:Configuration="Release" /p:Platform="AnyCPU"

@echo.
@echo ===== Creating NuGet package =====
@echo.
@NuGet\nuget.exe pack NuGet\WPFSpark.UWP.nuspec
