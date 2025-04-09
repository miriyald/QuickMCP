:: Install .NET (if not already installed)
echo Checking for .NET installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Installing .NET SDK...
    powershell -Command "Invoke-WebRequest -Uri https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1"
    powershell -Command ".\dotnet-install.ps1"
    del dotnet-install.ps1 /f /q
    echo .NET installed successfully.
) else (
    for /f "delims=" %%i in ('dotnet --version') do set dotnetInstalled=%%i
    echo .NET is already installed. Version: %dotnetInstalled%
)

:: Install QuickMCP.CLI tool
echo Installing QuickMCP.CLI globally...
dotnet tool install -g QuickMCP.CLI >nul 2>&1
if %errorlevel% equ 0 (
    echo QuickMCP.CLI installed successfully.
) else (
    echo Failed to install QuickMCP.CLI. Please check the error and try again.
    exit /b 1
)

:: Replace {serverID} with the actual server ID
set serverID="config.json"
echo Adding server with config file: %serverID%...
quickmcp server add %serverID% >nul 2>&1
if %errorlevel% equ 0 (
    echo Server added successfully.
) else (
    echo Failed to add server. Please check the error and try again.
    exit /b 1
)