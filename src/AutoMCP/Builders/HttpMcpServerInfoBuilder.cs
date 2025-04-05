using System.Text;
using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Http;
using AutoMCP.Models;
using Microsoft.Extensions.Logging;

namespace AutoMCP.Builders;
public abstract class HttpMcpServerInfoBuilder : BaseMcpServerInfoBuilder
{
    private IAuthenticator _authenticator;
    private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private HttpClient _httpClient;
    private HttpApiCaller _httpApiCaller;

    protected HttpMcpServerInfoBuilder(string serverName) : base(serverName)
    {
    }
    
    public HttpMcpServerInfoBuilder WithAuthenticator(IAuthenticator authenticator)
    {
        _authenticator = authenticator;
        return this;
    }
    public HttpMcpServerInfoBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }
    
    public HttpMcpServerInfoBuilder WithDefaultHeader(string key, string value)
    {
        _defaultHeaders[key] = value;
        return this;
    }

    // Sets the timeout for HTTP requests.
    public HttpMcpServerInfoBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
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
        
        _httpApiCaller = new HttpApiCaller(this._httpClient, this.Logger);
    }
}