using System.Text;
using System.Text.Json;
using AutoMCP.Abstractions;
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

    protected override MCPServerInfo CreateToolCollection()
    {
        var collection =  base.CreateToolCollection();
        BuildHttpCaller();
        collection.SetHttpCaller(_httpApiCaller);
        return collection;
    }

    protected void InitializeFromConfig(BuilderConfig config)
    {
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
                _defaultHeaders[header.Key] = header.Value;
            }
        }

        if (config.DefaultPathParameters != null)
        {
            foreach (var param in config.DefaultPathParameters)
            {
                _defaultPathParams[param.Key] = param.Value;
            }
        }
    }

    protected void BuildHttpCaller()
    {
        _httpClient ??= new HttpClient()
        {
            Timeout = this._timeout,
        };
        foreach (var headers in _defaultHeaders)
        {
            this._httpClient.DefaultRequestHeaders.Add(headers.Key, headers.Value);
        }
        
        _httpApiCaller = new HttpApiCaller(this._httpClient,  this.Logger);
        _httpApiCaller.SetBaseUrl(this._baseUrl);
        _httpApiCaller.SetDefaultPathParams(this._defaultPathParams);
        _httpApiCaller.SetAuthenticator(Authenticator);
    }
}