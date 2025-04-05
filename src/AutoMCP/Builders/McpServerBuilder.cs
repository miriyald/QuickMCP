using AutoMCP.Abstractions;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Builders;

public class McpServerInfoBuilder
{
    public static HttpMcpServerInfoBuilder ForOpenApi(string serverName = "openapi_tools")
    {
        return new OpenApiMcpServerInfoBuilder(serverName);
    }
    public static HttpMcpServerInfoBuilder ForGoogleDiscovery(string serverName = "google_api")
    {
        return new GoogleDiscoveryMcpServerInfoBuilder(serverName);
    }
}