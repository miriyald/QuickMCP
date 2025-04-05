using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace AutoMCP.Helpers
{
    public class MCPServer
    {
        private readonly string _serverName;
        private readonly ILogger<MCPServer> _logger;
        private readonly FastMCP _mcp;
        private readonly OAuthCache _oauthCache;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, ToolInfo> _registeredTools;
        private readonly Dictionary<string, ResourceInfo> _registeredResources;
        private  Dictionary<string, OperationInfo> _operationsInfo;
        private OpenApiDocument? _openApiSpec;
        private string? _apiCategory;

        public string ServerName => _serverName;
        public IReadOnlyDictionary<string, ToolInfo> RegisteredTools => _registeredTools;
        public IReadOnlyDictionary<string, ResourceInfo> RegisteredResources => _registeredResources;

        public MCPServer(string serverName = "openapi_proxy_server", ILoggerFactory? loggerFactory = null)
        {
            _serverName = serverName;
            _logger = loggerFactory?.CreateLogger<MCPServer>() ??
                      LoggerFactory.Create(builder => builder.AddConsole())
                          .CreateLogger<MCPServer>();
            _mcp = new FastMCP(serverName, loggerFactory);
            _oauthCache = new OAuthCache();
            _httpClient = new HttpClient();
            _registeredTools = new Dictionary<string, ToolInfo>();
            _registeredResources = new Dictionary<string, ResourceInfo>();
            _operationsInfo = new Dictionary<string, OperationInfo>();
        }

        private string SanitizeResourceName(string name, string? serverPrefix = null)
        {
            if (!string.IsNullOrEmpty(serverPrefix))
            {
                var prefixedName = $"{serverPrefix}_{name}";
                return StringHelpers.SanitizeName(prefixedName);
            }

            return StringHelpers.SanitizeName(name);
        }

        private JsonNode ConvertSchemaToResource(JsonNode? schema)
        {
            if (schema == null)
            {
                var emptyObject = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject()
                };
                return emptyObject;
            }

            var properties = new JsonObject();
            var requiredArray = schema["required"]?.AsArray();
            var schemaProperties = schema["properties"]?.AsObject();

            if (schemaProperties != null)
            {
                foreach (var property in schemaProperties)
                {
                    var propName = property.Key;
                    var propSchema = property.Value?.AsObject();
                    if (propSchema == null) continue;

                    var propType = propSchema["type"]?.GetValue<string>() ?? "string";
                    var propDescription = propSchema["description"]?.GetValue<string>() ?? "";

                    if (propType == "integer")
                    {
                        properties[propName] = new JsonObject
                        {
                            ["type"] = "number",
                            ["description"] = propDescription
                        };
                    }
                    else if (propType == "array")
                    {
                        var itemsType = ConvertSchemaToResource(propSchema["items"]?.AsObject());
                        properties[propName] = new JsonObject
                        {
                            ["type"] = "array",
                            ["items"] = itemsType,
                            ["description"] = propDescription
                        };
                    }
                    else if (propType == "object")
                    {
                        var nestedSchema = ConvertSchemaToResource(propSchema);
                        if (nestedSchema is JsonObject nestedObj)
                        {
                            nestedObj["description"] = propDescription;
                            properties[propName] = nestedObj;
                        }
                    }
                    else
                    {
                        properties[propName] = new JsonObject
                        {
                            ["type"] = propType,
                            ["description"] = propDescription
                        };
                    }
                }
            }

            var resourceSchema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties
            };

            if (requiredArray != null && requiredArray.Count > 0)
            {
                resourceSchema["required"] = requiredArray.DeepClone();
            }

            return resourceSchema;
        }

        public async Task<string?> GetOAuthAccessTokenAsync()
        {
            var token = _oauthCache.GetToken();
            if (token != null)
            {
                return token;
            }

            var clientId = Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET");
            var tokenUrl = Environment.GetEnvironmentVariable("OAUTH_TOKEN_URL");
            var scope = Environment.GetEnvironmentVariable("OAUTH_SCOPE") ?? "api";

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) &&
                !string.IsNullOrEmpty(tokenUrl))
            {
                try
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("scope", scope)
                    });

                    var response = await _httpClient.PostAsync(tokenUrl, content);
                    response.EnsureSuccessStatusCode();

                    var tokenData = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    var accessToken = tokenData.RootElement.GetProperty("access_token").GetString();
                    var expiresIn = tokenData.RootElement.TryGetProperty("expires_in", out var expiresInElement)
                        ? expiresInElement.GetInt32()
                        : 3600;

                    if (accessToken != null)
                    {
                        _oauthCache.SetToken(accessToken, expiresIn);
                        _logger.LogInformation("OAuth token obtained");
                        return accessToken;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error obtaining OAuth token");
                    Environment.Exit(1);
                }
            }

            _logger.LogInformation("No OAuth credentials; proceeding without token.");
            return null;
        }

        public async Task<(OpenApiDocument?, string, Dictionary<string, OperationInfo>)> LoadOpenApiAsync(
            string openApiUrl)
        {
            try
            {
                var res = await OpenApiDocument.LoadAsync(openApiUrl);
                var spec = res.Document;
                // var response = await _httpClient.GetAsync(openApiUrl);
                // response.EnsureSuccessStatusCode();
                //
                // var contentType = response.Content.Headers.ContentType?.MediaType;
                // OpenApiDocument? spec ;
                //
                // if (contentType != null && contentType.StartsWith("application/json"))
                // {
                //     
                //     var jsonContent = await response.Content.ReadAsStringAsync();
                //     var openApiReader = new OpenApiStringReader();
                //     var result = openApiReader.Read(jsonContent, out var diagnostic);
                //     spec = result;
                // }
                // else
                // {
                //     var content = await response.Content.ReadAsStreamAsync();
                //     var openApiReader = new OpenApiStreamReader();
                //     var result = openApiReader.Read(content, out var diagnostic);
                //     spec = result;
                // }

                if (spec == null || spec.Paths == null || spec.Info == null)
                {
                    _logger.LogError("Invalid OpenAPI spec: Missing required properties");
                    return (null, "", new Dictionary<string, OperationInfo>());
                }

                // Set API category from the spec info
                if (spec.Info != null)
                {
                    _apiCategory = spec.Info.Title?.Split(' ')[0];
                }

                var servers = spec.Servers;
                string rawUrl = "";
                var parsed = new Uri(openApiUrl);

                if (servers != null && servers.Count > 0)
                {
                    rawUrl = servers[0].Url;
                }

                if (string.IsNullOrEmpty(rawUrl))
                {
                    var basePath = Path.GetDirectoryName(parsed.AbsolutePath) ?? "";
                    rawUrl = $"{parsed.Scheme}://{parsed.Host}{basePath}";
                }

                string serverUrl;
                if (rawUrl.StartsWith("/"))
                {
                    serverUrl = $"{parsed.Scheme}://{parsed.Host}{rawUrl}";
                }
                else if (!rawUrl.StartsWith("http://") && !rawUrl.StartsWith("https://"))
                {
                    serverUrl = $"https://{rawUrl}";
                }
                else
                {
                    serverUrl = rawUrl;
                }

                var opsInfo = new Dictionary<string, OperationInfo>();

                foreach (var path in spec.Paths)
                {
                    foreach (var operation in path.Value.Operations)
                    {
                        var method = operation.Key.ToString().ToUpperInvariant();
                        var op = operation.Value;

                        var parameters = new List<Parameter>();

                        // Process path and query parameters
                        if (op.Parameters != null)
                        {
                            foreach (var param in op.Parameters)
                            {
                                parameters.Add(new Parameter
                                {
                                    Name = param.Name,
                                    In = param.In.ToString().ToLowerInvariant(),
                                    Required = param.Required,
                                    Schema = ConvertOpenApiSchemaToJsonNode(param.Schema),
                                    Description = param.Description ?? ""
                                });
                            }
                        }

                        // Process request body if present
                        if (op.RequestBody != null)
                        {
                            var requestBody = op.RequestBody;
                            var bodySchema = new JsonObject();

                            if (requestBody.Content.TryGetValue("application/json", out var mediaType) &&
                                mediaType.Schema != null)
                            {
                                bodySchema = ConvertOpenApiSchemaToJsonNode(mediaType.Schema);
                            }

                            parameters.Add(new Parameter
                            {
                                Name = "body",
                                In = "body",
                                Required = requestBody.Required,
                                Schema = bodySchema,
                                Description = "Request body"
                            });
                        }

                        // Extract response schema
                        JsonNode? responseSchema = null;
                        if (op.Responses != null && op.Responses.TryGetValue("200", out var response2))
                        {
                            if (response2.Content != null &&
                                response2.Content.TryGetValue("application/json", out var responseMediaType) &&
                                responseMediaType.Schema != null)
                            {
                                responseSchema = ConvertOpenApiSchemaToJsonNode(responseMediaType.Schema);
                            }
                        }

                        // Generate operation ID
                        var rawOpId = op.OperationId ??
                                      $"{method}_{path.Key.Replace("/", "_").Replace("{", "").Replace("}", "")}";
                        var sanitizedOpId = StringHelpers.SanitizeToolName(rawOpId);
                        var summary = op.Description ?? op.Summary ?? sanitizedOpId;

                        opsInfo[sanitizedOpId] = new OperationInfo
                        {
                            Summary = summary,
                            Parameters = parameters,
                            Path = path.Key,
                            Method = method,
                            ResponseSchema = responseSchema,
                            Tags = op.Tags?.Select(t => t.Name).ToList() ?? new List<string>()
                        };
                    }
                }

                _logger.LogInformation("Loaded {Count} operations from OpenAPI spec.", opsInfo.Count);
                return (spec, serverUrl, opsInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not load OpenAPI spec");
                return (null, "", new Dictionary<string, OperationInfo>());
            }
        }

        private JsonObject ConvertOpenApiSchemaToJsonNode(IOpenApiSchema? schema)
        {
            if (schema == null)
            {
                return new JsonObject();
            }

            var result = new JsonObject();
            result["type"] = schema.Type?.ToIdentifiers().FirstOrDefault()?? "object";

            if (schema.Description != null)
            {
                result["description"] = schema.Description;
            }

            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                var properties = new JsonObject();
                foreach (var prop in schema.Properties)
                {
                    properties[prop.Key] = ConvertOpenApiSchemaToJsonNode(prop.Value);
                }

                result["properties"] = properties;
            }

            if (schema.Items != null)
            {
                result["items"] = ConvertOpenApiSchemaToJsonNode(schema.Items);
            }

            if (schema.Required != null && schema.Required.Count > 0)
            {
                var requiredArray = new JsonArray();
                foreach (var req in schema.Required)
                {
                    requiredArray.Add(req);
                }

                result["required"] = requiredArray;
            }

            // Handle enum values
            if (schema.Enum != null && schema.Enum.Count > 0)
            {
                var enumArray = new JsonArray();
                foreach (var enumValue in schema.Enum)
                {
                   enumArray.Add(enumValue.DeepClone());
                    // Add other types as needed
                }

                result["enum"] = enumArray;
            }

            return result;
        }

        public ToolMetadata GetToolMetadata(Dictionary<string, OperationInfo> ops)
        {
            var tools = new List<Tool>();

            foreach (var (opId, info) in ops)
            {
                var prefixedOpId = $"{_serverName}_{opId}";
                var safeId = StringHelpers.SanitizeToolName(prefixedOpId);
                var properties = new JsonObject();
                var required = new List<string>();
                var parametersInfo = new List<ParameterInfo>();

                foreach (var param in info.Parameters)
                {
                    string name = param.Name;
                    var pSchema = param.Schema;
                    var pType = pSchema?["type"]?.GetValue<string>() ?? "string";
                    var desc = param.Description ?? $"Type: {pType}";

                    if (pSchema != null)
                    {
                        properties[name] = pSchema;
                    }
                    else
                    {
                        properties[name] = new JsonObject
                        {
                            ["type"] = pType,
                            ["description"] = desc
                        };
                    }

                    parametersInfo.Add(new ParameterInfo
                    {
                        Name = name,
                        In = param.In,
                        Required = param.Required,
                        Type = pType,
                        Description = desc
                    });

                    if (param.Required)
                    {
                        required.Add(name);
                    }
                }

                var schema = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = properties
                };

                if (required.Count > 0)
                {
                    var requiredArray = new JsonArray();
                    foreach (var req in required)
                    {
                        requiredArray.Add(req);
                    }

                    schema["required"] = requiredArray;
                }

                // Add tags from original operation plus our server name
                var tags = new List<string>(info.Tags);
                if (!string.IsNullOrEmpty(_apiCategory))
                {
                    tags.Add(_apiCategory);
                }

                tags.Add(_serverName);
                tags.Add("openapi");

                // Enhanced description with server context
                var enhancedDescription = $"[{_serverName}] {info.Summary}";

                var serverInfo = new JsonObject
                {
                    ["name"] = _serverName
                };

                var tool = new Tool
                {
                    Name = safeId,
                    Description = enhancedDescription,
                    InputSchema = schema,
                    Parameters = parametersInfo,
                    Tags = tags,
                    ServerInfo = serverInfo
                };

                if (info.ResponseSchema != null)
                {
                    tool.ResponseSchema = info.ResponseSchema;
                }

                tools.Add(tool);
            }

            return new ToolMetadata { Tools = tools };
        }

        public Dictionary<string, object?> ParseKwargsString(string s)
        {
            s = s.Trim();
            s = Regex.Replace(s, @"^`+|`+$", ""); // Remove surrounding backticks
            s = Regex.Replace(s, @"^```+|```+$", ""); // Remove surrounding triple backticks if present
            if (s.StartsWith('?'))
            {
                s = s[1..];
            }

            // Log the input string for debugging
            _logger.LogDebug("Parsing kwargs string: {String}", s);

            // Try standard JSON parsing first
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(s);
                if (parsed != null)
                {
                    _logger.LogDebug("Standard JSON parsing succeeded");
                    return parsed;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("Standard JSON parsing failed: {Error}", e.Message);
            }

            // Try with simple unescaping
            try
            {
                var sUnescaped = s.Replace("\\\"", "\"");
                var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(sUnescaped);
                if (parsed != null)
                {
                    _logger.LogDebug("JSON parsing with simple unescaping succeeded");
                    return parsed;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("JSON parsing with simple unescaping failed: {Error}", e.Message);
            }

            // Try with additional unescaping for backslashes
            try
            {
                var sDoubleUnescaped = s.Replace("\\\\", "\\");
                var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(sDoubleUnescaped);
                if (parsed != null)
                {
                    _logger.LogDebug("JSON parsing with double unescaping succeeded");
                    return parsed;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("JSON parsing with double unescaping failed: {Error}", e.Message);
            }

            // Try with both types of unescaping
            try
            {
                var sFullyUnescaped = s.Replace("\\\\", "\\").Replace("\\\"", "\"");
                var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(sFullyUnescaped);
                if (parsed != null)
                {
                    _logger.LogDebug("JSON parsing with full unescaping succeeded");
                    return parsed;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("JSON parsing with full unescaping failed: {Error}", e.Message);
            }

            // Extra attempt for the specific case where we have JSON inside a string
            var jsonPattern = @"(\{.*?\})";
            var jsonMatches = Regex.Matches(s, jsonPattern);
            if (jsonMatches.Count > 0)
            {
                foreach (Match match in jsonMatches)
                {
                    try
                    {
                        var jsonStr = match.Groups[1].Value;
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonStr);
                        if (parsed != null)
                        {
                            _logger.LogDebug("Extracted JSON substring parsing succeeded");
                            return parsed;
                        }
                    }
                    catch
                    {
                        // Continue to next match
                    }
                }
            }

            // Try standard query string parsing (expects '&' delimiter)
            try
            {
                var parsedQsl = HttpUtility.ParseQueryString(s);
                if (parsedQsl.Count > 0)
                {
                    var result = new Dictionary<string, object?>();
                    foreach (string key in parsedQsl.Keys)
                    {
                        if (key != null)
                        {
                            result[key] = parsedQsl[key];
                        }
                    }

                    _logger.LogDebug("Query string parsing succeeded");
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("Query string parsing failed: {Error}", e.Message);
            }

            // Fallback: if the string contains commas (but no '&'), split on commas manually.
            if (s.Contains(',') && !s.Contains('&'))
            {
                var result = new Dictionary<string, object?>();
                var pairs = s.Split(',');
                foreach (var pair in pairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                    {
                        continue;
                    }

                    if (trimmedPair.Contains('='))
                    {
                        var parts = trimmedPair.Split('=', 2);
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        // Try to convert values to appropriate types
                        try
                        {
                            // Try to convert to number if appropriate
                            if (double.TryParse(value, out var floatVal))
                            {
                                // Check if it's an integer
                                if (floatVal == Math.Floor(floatVal))
                                {
                                    result[key] = (int)floatVal;
                                }
                                else
                                {
                                    result[key] = floatVal;
                                }
                            }
                            else
                            {
                                result[key] = value;
                            }
                        }
                        catch
                        {
                            result[key] = value;
                        }
                    }
                }

                if (result.Count > 0)
                {
                    _logger.LogDebug("Comma-separated parsing succeeded");
                    return result;
                }
            }

            _logger.LogWarning("All parsing methods failed for string: {String}", s);
            return new Dictionary<string, object?>();
        }

        private (PreparedRequest?, JsonRpcResponse?) PrepareRequest(
            object requestId,
            Dictionary<string, object?> kwargs,
            List<Parameter> parameters,
            string path,
            string serverUrl,
            string opId,
            Dictionary<string, OperationInfo> ops)
        {
            if (kwargs.ContainsKey("kwargs"))
            {
                if (kwargs["kwargs"] is string rawKwargs)
                {
                    rawKwargs = Regex.Replace(rawKwargs, @"^`+|`+$", "");
                    _logger.LogInformation("Parsing kwargs string: {RawKwargs}", rawKwargs);
                    var parsedKwargs = ParseKwargsString(rawKwargs);
                    if (parsedKwargs.Count == 0)
                    {
                        _logger.LogWarning("Failed to parse kwargs string, returning error");
                        return (null, new JsonRpcResponse
                        {
                            JsonRpc = "2.0",
                            Id = requestId,
                            Error = new JsonRpcError
                            {
                                Code = -32602,
                                Message = $"Could not parse kwargs string: '{rawKwargs}'. Please check format."
                            }
                        });
                    }

                    kwargs.Remove("kwargs");
                    foreach (var kvp in parsedKwargs)
                    {
                        kwargs[kvp.Key] = kvp.Value;
                    }

                    _logger.LogInformation("Parsed kwargs: {Kwargs}", JsonSerializer.Serialize(kwargs));
                }
                else if (kwargs["kwargs"] is Dictionary<string, object?> kwargsDict)
                {
                    // If kwargs is already a dict, just use it directly
                    kwargs.Remove("kwargs");
                    foreach (var kvp in kwargsDict)
                    {
                        kwargs[kvp.Key] = kvp.Value;
                    }

                    _logger.LogInformation("Using provided kwargs dict: {Kwargs}", JsonSerializer.Serialize(kwargs));
                }
            }

            var expected = parameters
                .Where(p => p.Required)
                .Select(p => p.Name)
                .ToList();

            _logger.LogInformation("Expected required parameters: {Expected}", string.Join(", ", expected));
            _logger.LogInformation("Available parameters: {Available}", string.Join(", ", kwargs.Keys));

            var missing = expected.Where(name => !kwargs.ContainsKey(name)).ToList();
            if (missing.Count > 0)
            {
                return (null, new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = requestId,
                    Result = new Dictionary<string, object>
                        { ["help"] = $"Missing parameters: {string.Join(", ", missing)}" }
                });
            }

            var dryRun = false;
            if (kwargs.TryGetValue("dry_run", out var dryRunValue))
            {
                kwargs.Remove("dry_run");
                dryRun = Convert.ToBoolean(dryRunValue);
            }

            var reqParams = new Dictionary<string, string>();
            var reqHeaders = new Dictionary<string, string>();
            object? reqBody = null;

            foreach (var param in parameters)
            {
                var name = param.Name;
                var location = param.In ?? "query";

                if (kwargs.TryGetValue(name, out var value))
                {
                    try
                    {
                        var pSchema = param.Schema;
                        var pType = pSchema?["type"]?.GetValue<string>() ?? "string";

                        if (pType == "integer" && value != null)
                        {
                            value = Convert.ToInt32(value);
                        }
                        else if (pType == "number" && value != null)
                        {
                            value = Convert.ToDouble(value);
                        }
                        else if (pType == "boolean" && value != null)
                        {
                            var strValue = value.ToString()?.ToLowerInvariant();
                            value = strValue == "true" || strValue == "1" || strValue == "yes" || strValue == "y";
                        }

                        if (location == "path")
                        {
                            path = path.Replace($"{{{name}}}", value?.ToString() ?? "");
                        }
                        else if (location == "query")
                        {
                            reqParams[name] = value?.ToString() ?? "";
                        }
                        else if (location == "header")
                        {
                            reqHeaders[name] = value?.ToString() ?? "";
                        }
                        else if (location == "body")
                        {
                            reqBody = value;
                        }
                    }
                    catch (Exception e)
                    {
                        return (null, new JsonRpcResponse
                        {
                            JsonRpc = "2.0",
                            Id = requestId,
                            Error = new JsonRpcError
                            {
                                Code = -32602,
                                Message = $"Parameter '{name}' conversion error: {e.Message}"
                            }
                        });
                    }
                }
            }

            var token = GetOAuthAccessTokenAsync().Result;
            if (!string.IsNullOrEmpty(token))
            {
                reqHeaders["Authorization"] = $"Bearer {token}";
            }

            reqHeaders.TryAdd("User-Agent", "OpenAPI-MCP/1.0");
            var fullUrl = $"{serverUrl.TrimEnd('/')}/{path.TrimStart('/')}";

            return (new PreparedRequest(fullUrl, reqParams, reqHeaders, reqBody, dryRun), null);
        }

        private Func<object, Dictionary<string, object?>, Task<JsonRpcResponse>> GenerateToolFunction(
            string opId,
            string method,
            string path,
            List<Parameter> parameters,
            string serverUrl,
            Dictionary<string, OperationInfo> ops,
            HttpClient client)
        {
            if (opId == "updatePetWithForm")
            {
                
            }
            async Task<JsonRpcResponse> ToolFunc(object reqId, Dictionary<string, object?> kwargs)
            {
                var (prep, err) = PrepareRequest(reqId, kwargs, parameters, path, serverUrl, opId, ops);
                if (err != null)
                {
                    return err;
                }

                if (prep == null)
                {
                    return BuildResponse(reqId, error: new JsonRpcError
                    {
                        Code = -32603,
                        Message = "Failed to prepare request"
                    });
                }

                var (fullUrl, reqParams, reqHeaders, reqBody, dryRun) = prep;

                if (dryRun)
                {
                    return BuildResponse(reqId, result: new Dictionary<string, object>
                    {
                        ["dry_run"] = true,
                        ["request"] = new Dictionary<string, object?>
                        {
                            ["url"] = fullUrl,
                            ["method"] = method,
                            ["headers"] = reqHeaders,
                            ["params"] = reqParams,
                            ["body"] = reqBody
                        }
                    });
                }

                try
                {
                    var request = new HttpRequestMessage();
                    request.Method = new HttpMethod(method);

                    // Build URL with query parameters
                    var uriBuilder = new UriBuilder(fullUrl);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    foreach (var param in reqParams)
                    {
                        query[param.Key] = param.Value;
                    }

                    uriBuilder.Query = query.ToString();
                    request.RequestUri = uriBuilder.Uri;

                    // Add headers
                    foreach (var header in reqHeaders)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    // Add body content if present
                    if (reqBody != null)
                    {
                        var json = JsonSerializer.Serialize(reqBody);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    object data;
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != null && contentType.StartsWith("application/json"))
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        data = JsonSerializer.Deserialize<JsonElement>(responseJson);
                    }
                    else
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        data = new Dictionary<string, object> { ["raw_response"] = responseText };
                    }

                    return BuildResponse(reqId, result: new Dictionary<string, object> { ["data"] = data! });
                }
                catch (Exception e)
                {
                    return BuildResponse(reqId, error: new JsonRpcError
                    {
                        Code = -32603,
                        Message = e.Message
                    });
                }
            }

            return ToolFunc;
        }

        private JsonRpcResponse BuildResponse(object reqId, Dictionary<string, object>? result = null,
            JsonRpcError? error = null)
        {
            if (error != null)
            {
                return new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = reqId,
                    Error = error
                };
            }

            return new JsonRpcResponse
            {
                JsonRpc = "2.0",
                Id = reqId,
                Result = result
            };
        }

        public async Task<JsonRpcResponse> InitializeToolAsync(object reqId, Dictionary<string, object?> kwargs)
        {
            var serverDescription = _openApiSpec?.Info?.Description ?? $"OpenAPI Proxy for {_serverName}";
            var apiTitle = _openApiSpec?.Info?.Title ?? "API";

            return new JsonRpcResponse
            {
                JsonRpc = "2.0",
                Id = reqId,
                Result = new Dictionary<string, object>
                {
                    ["protocolVersion"] = "2024-11-05",
                    ["capabilities"] = new Dictionary<string, object>
                    {
                        ["tools"] = new Dictionary<string, object>
                        {
                            ["listChanged"] = true
                        }
                    },
                    ["serverInfo"] = new Dictionary<string, object>
                    {
                        ["name"] = _serverName,
                        ["version"] = "1.0.0",
                        ["description"] = $"OpenAPI Proxy for {apiTitle}: {serverDescription}",
                        ["category"] = _apiCategory ?? "API Integration",
                        ["tags"] = _apiCategory != null
                            ? new List<string> { "openapi", "api", _serverName, _apiCategory }
                            : new List<string> { "openapi", "api", _serverName }
                    }
                }
            };
        }

        public JsonRpcResponse ToolsListToolAsync(object reqId)
        {
            var toolList = _registeredTools.Values.Select(t => t.Metadata).ToList();
            return new JsonRpcResponse
            {
                JsonRpc = "2.0",
                Id = reqId,
                Result = new Dictionary<string, object>
                {
                    ["tools"] = toolList
                }
            };
        }

        public async Task<JsonRpcResponse> ToolsCallToolAsync(
            object reqId,
            string? name = null,
            Dictionary<string, object?>? arguments = null, string metadata = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = reqId,
                    Error = new JsonRpcError
                    {
                        Code = -32602,
                        Message = "Missing tool name"
                    }
                };
            }

            // Handle case where user forgot the server prefix
            if (!_registeredTools.ContainsKey(name))
            {
                var prefixedName = $"{_serverName}_{name}";
                if (_registeredTools.ContainsKey(prefixedName))
                {
                    name = prefixedName;
                }
                else
                {
                    return new JsonRpcResponse
                    {
                        JsonRpc = "2.0",
                        Id = reqId,
                        Error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = $"Tool '{name}' not found. Did you mean '{_serverName}_{name}'?"
                        }
                    };
                }
            }

            try
            {
                var func = _registeredTools[name].Function;
                return await func(reqId, arguments ?? new Dictionary<string, object?>());
            }
            catch (Exception e)
            {
                return new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = reqId,
                    Error = new JsonRpcError
                    {
                        Code = -32603,
                        Message = e.Message
                    }
                };
            }
        }

        public void AddTool(
            string name,
            Func<object, Dictionary<string, object?>, Task<JsonRpcResponse>> func,
            string description,
            Dictionary<string, object>? metadata = null)
        {
            // Add server name as prefix
            var prefixedName = $"{_serverName}_{name}";
            var safeName = StringHelpers.SanitizeToolName(prefixedName);

            // Enhance description with server context
            var enhancedDescription = $"[{_serverName}] {description}";

            Dictionary<string, object> finalMetadata;
            if (metadata == null)
            {
                finalMetadata = new Dictionary<string, object>
                {
                    ["name"] = safeName,
                    ["description"] = enhancedDescription,
                    ["tags"] = new List<string> { "openapi", "api", _serverName },
                    ["serverInfo"] = new Dictionary<string, string> { ["name"] = _serverName }
                };
            }
            else
            {
                finalMetadata = new Dictionary<string, object>(metadata)
                {
                    ["name"] = safeName,
                    ["description"] = enhancedDescription
                };

                // Add tags if not present
                if (!finalMetadata.ContainsKey("tags"))
                {
                    finalMetadata["tags"] = new List<string> { "openapi", "api", _serverName };
                }

                // Add server info
                finalMetadata["serverInfo"] = new Dictionary<string, string> { ["name"] = _serverName };
            }

            _registeredTools[safeName] = new ToolInfo
            {
                Function = func,
                Metadata = finalMetadata
            };

            _mcp.AddTool(safeName, enhancedDescription, func);
        }

        public async Task RegisterOpenApiToolsAsync()
        {
            var openApiUrl = "https://petstore.swagger.io/v2/swagger.json";
            if (!string.IsNullOrEmpty(openApiUrl))
            {
                try
                {
                    _logger.LogInformation("Loading OpenAPI spec from: {OpenApiUrl}", openApiUrl);
                    var (spec, serverUrl, opsInfo) = await LoadOpenApiAsync(openApiUrl);
                    _openApiSpec = spec;
                    _operationsInfo = opsInfo;

                    if (spec != null)
                    {
                        // Log OpenAPI info
                        var apiTitle = spec.Info?.Title ?? "Unknown API";
                        var apiVersion = spec.Info?.Version ?? "Unknown version";
                        _logger.LogInformation("Loaded API: {ApiTitle} (version: {ApiVersion})", apiTitle, apiVersion);

                        // Register resources and count them
                        var resourceCount = RegisterOpenApiResources();
                        _logger.LogInformation("Registered {ResourceCount} resources from OpenAPI spec", resourceCount);

                        // Register tools and count successful registrations
                        var toolCount = 0;
                        foreach (var (opId, info) in opsInfo)
                        {
                            try
                            {
                                var client = new HttpClient();
                                var toolMeta = GetToolMetadata(new Dictionary<string, OperationInfo> { [opId] = info })
                                    .Tools[0];

                                var func = GenerateToolFunction(
                                    opId,
                                    info.Method,
                                    info.Path,
                                    info.Parameters,
                                    serverUrl,
                                    opsInfo,
                                    client);

                                AddTool(opId, func, info.Summary, toolMeta.ToDictionary());
                                toolCount++;
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to register tool for operation {OperationId}", opId);
                            }
                        }

                        _logger.LogInformation("Successfully registered {ToolCount}/{TotalOps} API tools", toolCount,
                            opsInfo.Count);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to load OpenAPI spec from {OpenApiUrl}", openApiUrl);
                }
            }
            else
            {
                _logger.LogWarning("No OPENAPI_URL provided, skipping API tools registration");
            }

            // Register standard MCP tools
            AddTool("initialize", InitializeToolAsync, "Initialize MCP server.");
            AddTool("tools_list", (reqId, kwargs) => Task.FromResult(ToolsListToolAsync(reqId)),
                "List available tools with extended metadata.");
           // AddTool("tools_call", ToolsCallToolAsync, "Call a tool by name with provided arguments.");
            _logger.LogInformation("Registered 3 standard MCP tools");
        }

        public int RegisterOpenApiResources()
        {
            if (_openApiSpec?.Components?.Schemas == null)
            {
                return 0;
            }

            var schemas = _openApiSpec.Components.Schemas;
            var resourceCount = 0;

            foreach (var (schemaName, schema) in schemas)
            {
                // Prefix resource name with server name
                var prefixedName = $"{_serverName}_{schemaName}";
                var safeName = SanitizeResourceName(prefixedName);

                var schemaNode = ConvertOpenApiSchemaToJsonNode(schema);
                var resourceSchema = ConvertSchemaToResource(schemaNode);
                var resourceDescription = $"[{_serverName}] {schema.Description ?? $"Resource for {schemaName}"}";

                var resourceObj = new MCPResource(
                    name: safeName,
                    schema: resourceSchema,
                    description: resourceDescription
                );

                _mcp.AddResource(resourceObj);

                var tags = new List<string> { "resource", _serverName };
                if (!string.IsNullOrEmpty(_apiCategory))
                {
                    tags.Add(_apiCategory);
                }

                _registeredResources[safeName] = new ResourceInfo
                {
                    Schema = resourceSchema,
                    Metadata = new Dictionary<string, object>
                    {
                        ["name"] = safeName,
                        ["description"] = resourceDescription,
                        ["serverInfo"] = new Dictionary<string, string> { ["name"] = _serverName },
                        ["tags"] = tags
                    }
                };

                resourceCount++;
            }

            return resourceCount;
        }

        public int GenerateApiPrompts()
        {
            if (_openApiSpec == null)
            {
                return 0;
            }

            var info = _openApiSpec.Info;
            var apiTitle = info?.Title ?? "API";
            var generalPrompt = new StringBuilder();

            generalPrompt.AppendLine($@"
# {_serverName} - API Usage Guide for {apiTitle}

This API provides the following capabilities:
");

            if (_openApiSpec.Paths != null)
            {
                foreach (var (path, pathItem) in _openApiSpec.Paths)
                {
                    foreach (var (methodEnum, operation) in pathItem.Operations)
                    {
                        var method = methodEnum.ToString().ToLowerInvariant();
                        if (new[] { "get", "post", "put", "delete", "patch" }.Contains(method))
                        {
                            var rawToolName = operation.OperationId ?? $"{method}_{path}";
                            var toolName = $"{_serverName}_{rawToolName}";

                            generalPrompt.AppendLine($"\n## {toolName}");
                            generalPrompt.AppendLine($"- Path: `{path}` (HTTP {method.ToUpperInvariant()})");
                            generalPrompt.AppendLine(
                                $"- Description: {operation.Description ?? operation.Summary ?? "No description"}");

                            if (operation.Parameters?.Count > 0)
                            {
                                generalPrompt.AppendLine("- Parameters:");
                                foreach (var param in operation.Parameters)
                                {
                                    var required = param.Required ? "Required" : "Optional";
                                    generalPrompt.AppendLine(
                                        $"  - `{param.Name}` ({param.In}): {param.Description ?? "No description"} [{required}]");
                                }
                            }
                        }
                    }
                }
            }

            var promptName = $"{_serverName}_api_general_usage";
            var promptDescription = $"[{_serverName}] General guidance for using {apiTitle} API";
            var prompt = new Prompt(promptName, generalPrompt.ToString(), promptDescription);
            _mcp.AddPrompt(prompt);

            return 1; // Return number of prompts created
        }

        public Dictionary<string, Dictionary<string, string>> IdentifyCrudOperations()
        {
            var crudOps = new Dictionary<string, Dictionary<string, string>>();

            if (_openApiSpec?.Paths == null)
            {
                return crudOps;
            }

            foreach (var (path, pathItem) in _openApiSpec.Paths)
            {
                var pathParts = path.Split('/')
                    .Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("{"))
                    .ToList();

                if (pathParts.Count == 0)
                {
                    continue;
                }

                var resource = StringHelpers.SingularizeResource(pathParts[^1]);
                if (!crudOps.ContainsKey(resource))
                {
                    crudOps[resource] = new Dictionary<string, string>();
                }

                foreach (var (methodEnum, operation) in pathItem.Operations)
                {
                    var method = methodEnum.ToString().ToLowerInvariant();
                    var opId = StringHelpers.SanitizeToolName(
                        operation.OperationId ?? $"{method}_{path.Replace("/", "_").Replace("{", "").Replace("}", "")}"
                    );

                    if (method == "get")
                    {
                        if (path.Contains("{") && path.Contains("}"))
                        {
                            crudOps[resource]["get"] = opId;
                        }
                        else
                        {
                            crudOps[resource]["list"] = opId;
                        }
                    }
                    else if (method == "post")
                    {
                        crudOps[resource]["create"] = opId;
                    }
                    else if (method == "put" || method == "patch")
                    {
                        crudOps[resource]["update"] = opId;
                    }
                    else if (method == "delete")
                    {
                        crudOps[resource]["delete"] = opId;
                    }
                }
            }

            return crudOps;
        }

        public int GenerateExamplePrompts()
        {
            var crudOps = IdentifyCrudOperations();
            var promptCount = 0;

            foreach (var (resource, operations) in crudOps)
            {
                var examplePrompt = new StringBuilder();
                examplePrompt.AppendLine($@"
# {_serverName} - Examples for working with {resource}

Common scenarios for handling {resource} resources:
");

                if (operations.TryGetValue("list", out var listOp))
                {
                    var prefixedOp = $"{_serverName}_{listOp}";
                    examplePrompt.AppendLine($@"
## Listing {resource} resources

To list all {resource} resources:
```
{{{{tool.{prefixedOp}()}}}}
```
");
                }

                if (operations.TryGetValue("get", out var getOp))
                {
                    var prefixedOp = $"{_serverName}_{getOp}";
                    examplePrompt.AppendLine($@"
## Getting a specific {resource}

To retrieve a specific {resource} by ID:
```
{{{{tool.{prefixedOp}(id=""example-id"")}}}}
```
");
                }

                if (operations.TryGetValue("create", out var createOp))
                {
                    var prefixedOp = $"{_serverName}_{createOp}";
                    examplePrompt.AppendLine($@"
## Creating a new {resource}

To create a new {resource}:
```
{{{{tool.{prefixedOp}(
    name=""Example name"",
    description=""Example description""
    # Add other required fields
)}}}}
```
");
                }

                var promptName = $"{_serverName}_{resource}_examples";
                var promptDescription = $"[{_serverName}] Example usage patterns for {resource} resources";
                var prompt = new Prompt(promptName, examplePrompt.ToString(), promptDescription);
                _mcp.AddPrompt(prompt);
                promptCount++;
            }

            return promptCount;
        }

        public async Task RunAsync()
        {
            await _mcp.RunAsync("stdio");
        }
    }
}