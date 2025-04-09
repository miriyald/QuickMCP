
#!/bin/bash

# Install .NET (if not already installed)
echo "Checking for .NET installation..."
if ! command -v dotnet &> /dev/null
then
    echo ".NET not found. Installing .NET SDK..."
    curl -sSL https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh
    rm -f dotnet-install.sh
    echo ".NET installed successfully."
else
    dotnetVersion=$(dotnet --version)
    echo ".NET is already installed. Version: $dotnetVersion"
fi

# Install QuickMCP.CLI tool
echo "Installing QuickMCP.CLI globally..."
dotnet tool install -g QuickMCP.CLI
if [ $? -eq 0 ]; then
    echo "QuickMCP.CLI installed successfully."
else
    echo "Failed to install QuickMCP.CLI. Please check the error and try again."
    exit 1
fi

# Replace {serverID} with the actual server ID
serverID="config.json" # Replace with the actual server ID
echo "Adding server with config file: $serverID..."
quickmcp server add "$serverID"
if [ $? -eq 0 ]; then
    echo "Server added successfully."
else
    echo "Failed to add server. Please check the error and try again."
    exit 1
fi