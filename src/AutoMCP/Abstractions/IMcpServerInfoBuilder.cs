using AutoMCP.Models;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Abstractions;

/// <summary>
/// Interface for building MCP tools from various sources
/// </summary>
public interface IMcpServerInfoBuilder
{
    /// <summary>
    /// Creates tools from a Swagger/OpenAPI URL
    /// </summary>
    /// <param name="url">The URL to the Swagger/OpenAPI specification</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder FromUrl(string url);

    /// <summary>
    /// Creates tools from a Swagger/OpenAPI file on disk
    /// </summary>
    /// <param name="filePath">Path to the Swagger/OpenAPI specification file</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder FromFile(string filePath);

    /// <summary>
    /// Creates tools from a configuration file
    /// </summary>
    /// <param name="configPath">Path to the configuration JSON file</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder FromConfiguration(string configPath);

    /// <summary>
    /// Excludes paths or operations from being registered as tools
    /// </summary>
    /// <param name="exclusionPredicate">Predicate function that returns true for paths to exclude</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder ExcludePaths(Func<string, bool> exclusionPredicate);

    /// <summary>
    /// Excludes specific operation IDs from being registered as tools
    /// </summary>
    /// <param name="paths">List of paths to exclude</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder ExcludePaths(IEnumerable<string> paths);

    /// <summary>
    /// Includes only paths that match the provided predicate
    /// </summary>
    /// <param name="inclusionPredicate">Predicate function that returns true for paths to include</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder OnlyForPaths(Func<string, bool> inclusionPredicate);

    /// <summary>
    /// Includes only the specified operation IDs
    /// </summary>
    /// <param name="paths">List of operation IDs to include</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder OnlyForPaths(IEnumerable<string> paths);

    /// <summary>
    /// Adds authentication for API requests
    /// </summary>
    /// <param name="authenticator">The authenticator to use for requests</param>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder AddAuthentication(IAuthenticator authenticator);


    /// <summary>
    /// Configures the builder to use logging for operations and events.
    /// </summary>
    /// <returns>A builder instance for method chaining</returns>
    IMcpServerInfoBuilder UseLogging(ILogger logger);

    /// <summary>
    /// Builds and registers all configured tools with the MCP server
    /// </summary>
    /// <returns>Task that completes when tools are registered</returns>
    Task<MCPServerInfo> BuildAsync();
}