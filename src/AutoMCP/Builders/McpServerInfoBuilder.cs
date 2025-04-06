using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Types;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Builders;

/// <summary>
/// Provides methods to create instances of <see cref="HttpMcpServerInfoBuilder"/> 
/// for different server types, such as OpenAPI and Google Discovery API.
/// </summary>
public class McpServerInfoBuilder
{
    /// <summary>
    /// Creates an <see cref="OpenApiMcpServerInfoBuilder"/> instance 
    /// for building server information from OpenAPI/Swagger specifications.
    /// </summary>
    /// <param name="serverName">The name of the server.</param>
    /// <returns>An instance of <see cref="OpenApiMcpServerInfoBuilder"/>.</returns>
    public static HttpMcpServerInfoBuilder ForOpenApi(string serverName = "openapi_tools")
    {
        return new OpenApiMcpServerInfoBuilder(serverName);
    }

    /// <summary>
    /// Creates a <see cref="GoogleDiscoveryMcpServerInfoBuilder"/> instance 
    /// for building server information from Google Discovery API specifications.
    /// </summary>
    /// <param name="serverName">The name of the server.</param>
    /// <returns>An instance of <see cref="GoogleDiscoveryMcpServerInfoBuilder"/>.</returns>
    public static HttpMcpServerInfoBuilder ForGoogleDiscovery(string serverName = "google_api")
    {
        return new GoogleDiscoveryMcpServerInfoBuilder(serverName);
    }

    public static IMcpServerInfoBuilder FromConfig(string configFile)
    {
        if(string.IsNullOrEmpty(configFile))
            throw new ArgumentNullException(nameof(configFile));
        var file = File.ReadAllText(configFile);
        var config = (BuilderConfig) JsonSerializer.Deserialize(configFile, AutoMcpJsonSerializerContext.Default.BuilderConfig);

        return FromConfig(config);
    }

    public static IMcpServerInfoBuilder FromConfig(BuilderConfig config)
    {
        return config.Type switch
        {
            "openapi" => ForOpenApi().WithConfig(config),
            "discovery" => ForGoogleDiscovery().WithConfig(config),
            _ => throw new ArgumentException(
                $"Invalid server type: {config.Type}. Supported types are: openapi, discovery")
        };
    }
}