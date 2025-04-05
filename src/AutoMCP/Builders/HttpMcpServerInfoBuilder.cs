using System.Text;
using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Authentication;
using AutoMCP.Http;
using AutoMCP.Models;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Builders;
public abstract class HttpMcpServerInfoBuilder : BaseMcpServerInfoBuilder
{
    private HttpApiCaller _httpApiCaller;
    protected HttpMcpServerInfoBuilder(string serverName) : base(serverName)
    {
    }
    /// <inheritdoc/>
    protected override McpServerInfo CreateToolCollection()
    {
        var collection =  base.CreateToolCollection();
        BuildHttpCaller();
        collection.SetHttpCaller(_httpApiCaller);
        return collection;
    }

    /// <inheritdoc/>
    public override IMcpServerInfoBuilder FromConfiguration(string configPath)
    {
        base.FromConfiguration(configPath);
        var config = LoadConfigurationAsync<BuilderConfig>(configPath).Result;
        if (config != null)
        {
            InitializeFromConfig(config);   
        }
        return this;
    }


    /// <summary>
    /// Initializes the builder's configuration settings from the specified configuration object.
    /// This method sets various properties such as server name, description, headers, paths, and authentication.
    /// </summary>
    /// <param name="config">A <see cref="BuilderConfig"/> instance containing the configuration settings to initialize the builder.</param>
    protected void InitializeFromConfig(BuilderConfig config)
    {
        this._config = config;
        // Use server name and description from config if provided
        if (!string.IsNullOrEmpty(config.ServerName))
        {
            ServerName = config.ServerName;
        }

        if (!string.IsNullOrEmpty(config.ServerDescription))
        {
            ServerDescription = config.ServerDescription;
        }

        if(!string.IsNullOrEmpty(config.ApiBaseUrl))
            SetBaseUrl(config.ApiBaseUrl);
        
        // Set resource and prompt generation flags from config
        GenerateResourcesFlag = config.GenerateResources;
        GeneratePromptsFlag = config.GeneratePrompts;

        if (config.ExcludedPaths != null)
            ExcludePaths(config.ExcludedPaths);
        if (config.IncludedPaths != null)
            OnlyForPaths(config.IncludedPaths);
        if (config.ServerHeaders != null)
        {
            foreach (var header in config.ServerHeaders)
            {
                DefaultHeaders[header.Key] = header.Value;
            }
        }

        if (config.DefaultPathParameters != null)
        {
            foreach (var param in config.DefaultPathParameters)
            {
                DefaultPathParams[param.Key] = param.Value;
            }
        }
        if (config.Authentication != null)
        {
            Authenticator = AuthenticatorFactory.Create(config.Authentication);
        }
    }

    /// <summary>
    /// Configures and initializes the HTTP API caller component of the MCP server builder.
    /// This method sets up the HttpClient with default headers, timeout settings, and assigns
    /// the base URL, default path parameters, and authenticator to the HTTP API caller instance.
    /// </summary>
    protected void BuildHttpCaller()
    {
        _httpClient ??= new HttpClient()
        {
            Timeout = this._timeout,
        };
        foreach (var headers in DefaultHeaders)
        {
            this._httpClient.DefaultRequestHeaders.Add(headers.Key, headers.Value);
        }
        
        _httpApiCaller = new HttpApiCaller(this._httpClient,  this.Logger);
        _httpApiCaller.SetBaseUrl(this.BaseUrl);
        _httpApiCaller.SetDefaultPathParams(this.DefaultPathParams);
        _httpApiCaller.SetAuthenticator(Authenticator);
    }
}