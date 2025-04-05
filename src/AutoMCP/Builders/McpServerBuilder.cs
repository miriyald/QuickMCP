using AutoMCP.Abstractions;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Builders;

public class McpServerBuilder
{
    public static IMcpServerInfoBuilder ForOpenApi(string serverName = "openapi_tools")
    {
        return new OpenApiMcpServerInfoBuilder(serverName);
    }
    public static IMcpServerInfoBuilder ForGoogleDiscovery(string serverName = "google_api")
    {
        return new GoogleDiscoveryMcpServerInfoBuilder(serverName);
    }
}