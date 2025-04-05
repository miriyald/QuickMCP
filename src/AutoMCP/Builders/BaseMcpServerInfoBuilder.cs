using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Helpers;
using AutoMCP.Http;
using AutoMCP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;

namespace AutoMCP.Builders;

/// <summary>
/// Base class for MCP tool builders that provides common functionality
/// </summary>
public abstract class BaseMcpServerInfoBuilder : IMcpServerInfoBuilder
{
    static BaseMcpServerInfoBuilder()
    {
        OpenApiReaderRegistry.RegisterReader("yaml", new OpenApiYamlReader());
    }

    protected string ServerName;
    protected string ServerDescription;
    protected ILogger? Logger;
    protected readonly HttpClient HttpClient;
    protected readonly Dictionary<string, Prompt> Prompts;

    protected IAuthenticator? Authenticator;
    protected Func<string, bool>? PathExclusionFunc;
    protected Func<string, bool>? PathInclusionFunc;
    protected List<string> ExcludedOperationIds = new List<string>();
    protected List<string> IncludedOperationIds = new List<string>();
    protected bool GenerateResourcesFlag = true;
    protected bool GeneratePromptsFlag = true;

    protected readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
    protected readonly Dictionary<string, string> _defaultPathParams = new Dictionary<string, string>();
    protected TimeSpan _timeout = TimeSpan.FromSeconds(30);
    protected HttpClient _httpClient;

    protected string _baseUrl;

    protected readonly Dictionary<string, ToolInfo> RegisteredTools;
    protected readonly Dictionary<string, ResourceInfo> RegisteredResources;

    /// <summary>
    /// Creates a new instance of BaseMCPToolBuilder
    /// </summary>
    /// <param name="serverName">Name of the server</param>
    /// <param name="loggerFactory">Optional logger factory</param>
    protected BaseMcpServerInfoBuilder(string serverName)
    {
        ServerName = serverName;
        ServerDescription = $"MCP Tools for {serverName}";
        HttpClient = new HttpClient();
        Prompts = new Dictionary<string, Prompt>();
        RegisteredTools = new Dictionary<string, ToolInfo>();
        RegisteredResources = new Dictionary<string, ResourceInfo>();
    }

    /// <inheritdoc />
    public abstract IMcpServerInfoBuilder FromUrl(string url);

    /// <inheritdoc />
    public abstract IMcpServerInfoBuilder FromFile(string filePath);

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder FromConfiguration(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Configuration file not found", configPath);
        }

        Logger?.LogInformation("Set configuration file path: {ConfigPath}", configPath);

        // Load configuration will happen in BuildAsync
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder ExcludePaths(Func<string, bool> exclusionPredicate)
    {
        PathExclusionFunc = exclusionPredicate;
        Logger?.LogInformation("Set path exclusion function");
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder ExcludePaths(IEnumerable<string> paths)
    {
        ExcludedOperationIds.AddRange(paths);
        Logger?.LogInformation("Excluded {Count} operation IDs", ExcludedOperationIds.Count);
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder OnlyForPaths(Func<string, bool> inclusionPredicate)
    {
        PathInclusionFunc = inclusionPredicate;
        Logger?.LogInformation("Set path inclusion function");
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder OnlyForPaths(IEnumerable<string> operationIds)
    {
        IncludedOperationIds.AddRange(operationIds);
        Logger?.LogInformation("Including only {Count} operation IDs", IncludedOperationIds.Count);
        return this;
    }

    public IMcpServerInfoBuilder SetBaseUrl(string baseUrl)
    {
        this._baseUrl = baseUrl;
        return this;
    }

    public IMcpServerInfoBuilder WithDefaultPathParams(Dictionary<string, string> defaultPathParams)
    {
        foreach (var param in defaultPathParams)
        {
            this._defaultPathParams[param.Key] = param.Value;
        }

        return this;
    }

    public IMcpServerInfoBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    public IMcpServerInfoBuilder WithDefaultHeader(string key, string value)
    {
        _defaultHeaders[key] = value;
        return this;
    }

    // Sets the timeout for HTTP requests.
    public IMcpServerInfoBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder AddAuthentication(IAuthenticator authenticator)
    {
        Authenticator = authenticator;
        Logger?.LogInformation("Added authenticator of type {AuthenticatorType}", authenticator.GetType().Name);
        return this;
    }

    /// <inheritdoc />
    public IMcpServerInfoBuilder UseLogging(ILogger _logger)
    {
        this.Logger = _logger;
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder GenerateResources(bool enabled = true)
    {
        GenerateResourcesFlag = enabled;
        Logger?.LogInformation("Resource generation {Status}", enabled ? "enabled" : "disabled");
        return this;
    }

    /// <inheritdoc />
    public virtual IMcpServerInfoBuilder GeneratePrompts(bool enabled = true)
    {
        GeneratePromptsFlag = enabled;
        Logger?.LogInformation("Prompt generation {Status}", enabled ? "enabled" : "disabled");
        return this;
    }

    /// <inheritdoc />
    public abstract Task<MCPServerInfo> BuildAsync();

    /// <summary>
    /// Filters a collection of operations based on the configured filters
    /// </summary>
    /// <typeparam name="T">The type of operations</typeparam>
    /// <param name="operations">Dictionary of operations to filter</param>
    /// <returns>The filtered dictionary</returns>
    protected virtual Dictionary<string, OperationInfo> FilterOperations(Dictionary<string, OperationInfo> operations)
    {
        var result = new Dictionary<string, OperationInfo>(operations);

        // Apply exclusion predicate
        if (PathExclusionFunc != null)
        {
            var keysToRemove = new List<string>();
            foreach (var key in result.Keys)
            {
                if (PathExclusionFunc(result[key].Path))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }
        }

        // Apply inclusion predicate
        if (PathInclusionFunc != null)
        {
            var keysToKeep = new List<string>();
            foreach (var key in result.Keys)
            {
                if (PathInclusionFunc(result[key].Path))
                {
                    keysToKeep.Add(key);
                }
            }

            var keysToRemove = new List<string>();
            foreach (var key in result.Keys)
            {
                if (!keysToKeep.Contains(key))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }
        }

        // Apply excluded operation IDs
        if (ExcludedOperationIds != null && ExcludedOperationIds.Count > 0)
        {
            foreach (var opId in result.Keys)
            {
                var value = result[opId];
                if (ExcludedOperationIds.Any(s => value.Path.ToLower().Contains(s.ToLower())))
                    result.Remove(opId);
            }
        }

        // Apply included operation IDs
        if (IncludedOperationIds != null && IncludedOperationIds.Count > 0)
        {
            var keysToRemove = new List<string>();
            foreach (var key in result.Keys)
            {
                var value = result[key];
                if (value is OperationInfo opInfo)
                {
                    if (!IncludedOperationIds.Any(s => opInfo.Path.ToLower().Contains(s.ToLower())))
                    {
                        keysToRemove.Add(key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }
        }

        return result;
    }

    /// <summary>
    /// Adds a tool to the collection of registered tools
    /// </summary>
    /// <param name="name">Name of the tool</param>
    /// <param name="function">Function that implements the tool</param>
    /// <param name="description">Description of the tool</param>
    /// <param name="metadata">Additional metadata for the tool</param>
    protected virtual void AddTool(
        string name,
        ToolInfo function,
        string description,
        ToolMetadata? metadata = null)
    {
        // Add server name as prefix
        var prefixedName = $"{ServerName}_{name}";
        var safeName = StringHelpers.SanitizeToolName(prefixedName);

        // Enhance description with server context
        var enhancedDescription = $"[{ServerName}] {description}";

        ToolMetadata finalMetadata;
        if (metadata == null)
        {
            finalMetadata = new ToolMetadata()
            {
                Name = safeName,
                Description = enhancedDescription,
                Tags = new List<string> { "api", ServerName },
                ServerInfo = new ServerInfo() { Name = ServerName }
            };
        }
        else
        {
            finalMetadata = metadata;
            finalMetadata.Name = safeName;
            finalMetadata.Description = enhancedDescription;


            // Add tags if not present
            if (finalMetadata.Tags.Count == 0)
            {
                finalMetadata.Tags = new List<string> { "api", ServerName };
            }

            // Add server info
            finalMetadata.ServerInfo = new ServerInfo() { Name = ServerName };
        }

        function.Name = metadata.Name ?? safeName;
        function.Metadata = finalMetadata;
        RegisteredTools[safeName] = function;
    }

    /// <summary>
    /// Creates a standard tool function for making API requests
    /// </summary>
    /// <param name="method">HTTP method (GET, POST, etc.)</param>
    /// <param name="url">URL template for the request</param>
    /// <param name="parameters">List of parameters for the request</param>
    /// <returns>Function that handles the tool request</returns>
    protected virtual ToolInfo CreateToolFunction(
        string method, string url, List<Parameter> parameters)
    {
        string contentType = "application/json";
        if (method == "POST")
        {
            var param = parameters.FirstOrDefault(s => s.In == "body");
            if (param != null)
            {
                contentType = param.ContentType ?? "application/json";
            }
        }

        var toolInfo = new ToolInfo()
        {
            Method = method,
            Url = url,
            MimeType = contentType,
            Parameters = parameters
        };
        return toolInfo;
    }

    /// <summary>
    /// Loads configuration from a JSON file
    /// </summary>
    /// <param name="configPath">Path to the configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    protected virtual async Task<T?> LoadConfigurationAsync<T>(string configPath) where T : class
    {
        try
        {
            Logger?.LogInformation("Loading configuration from: {ConfigPath}", configPath);

#if NETSTANDARD2_0_OR_GREATER
            var configJson = File.ReadAllText(configPath);
#else
            var configJson = await File.ReadAllTextAsync(configPath);
#endif
            var config = (T?)JsonSerializer.Deserialize(configJson, typeof(T), AutoMcpJsonSerializerContext.Default);

            if (config == null)
            {
                Logger?.LogError("Invalid configuration file format");
            }

            return config;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to load configuration from: {ConfigPath}", configPath);
            return null;
        }
    }

    /// <summary>
    /// Creates a MCPToolCollection from the current state
    /// </summary>
    /// <returns>A collection of the built tools, resources, and prompts</returns>
    protected virtual MCPServerInfo CreateToolCollection()
    {
        return new MCPServerInfo(
            ServerName,
            ServerDescription,
            RegisteredTools,
            GenerateResourcesFlag ? RegisteredResources : new Dictionary<string, ResourceInfo>(),
            GeneratePromptsFlag ? Prompts : new Dictionary<string, Prompt>()
        );
    }
}