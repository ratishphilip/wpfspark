@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.6 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_6" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.6

@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.6.1 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_6_1" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.6.1

@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.6.2 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_6_2" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.6.2

@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.7 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_7" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.7

@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.7.1 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_7_1" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.7.1

@echo.
@echo ===== Building WPFSpark.dll (AnyCPU) - dotnet version 4.7.2 =====
@echo.
@msbuild WPFSpark\WPFSpark.csproj /p:Configuration="r_4_7_2" /p:Platform="AnyCPU" /p:TargetFrameworkVersion=v4.7.2

@echo.
@echo ===== Updating Demo folder =====
@echo.
@msbuild WPFSparkClient\WPFSparkClient.csproj /p:Configuration="Release" /p:Platform="AnyCPU"
@echo.
@echo ===== Copying Executable to 'Demo' folder =====
copy /Y "WPFSparkClient\bin\Release\WPFSparkClient.exe" "Demo"
@echo.
@echo ===== Copying Dependencies to '$(SolutionDir)Demo' folder =====
copy /Y "WPFSpark\bin\r_4_7_2\*.dll" "Demo"

@echo.
@echo.
@echo ===== Creating NuGet package =====
@echo.
@nuget.exe pack nuget\WPFSpark.nuspec
