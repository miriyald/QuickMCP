using QuickMCP.Http;
using QuickMCP.Server;
using ModelContextProtocol.Server;

namespace QuickMCP.Types;

/// <summary>
/// Collection of MCP tools, resources, and prompts
/// </summary>
public class McpServerInfo
{
    /// <summary>
    /// The name of the server/collection
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the server/collection
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Dictionary of registered tools
    /// </summary>
    public IReadOnlyDictionary<string, ToolInfo> Tools { get; }

    /// <summary>
    /// Dictionary of registered resources
    /// </summary>
    public IReadOnlyDictionary<string, ResourceInfo> Resources { get; }

    /// <summary>
    /// Dictionary of registered prompts
    /// </summary>
    public IReadOnlyDictionary<string, Prompt> Prompts { get; }

    /// <summary>
    /// Count of registered tools
    /// </summary>
    public int ToolCount => Tools.Count;

    /// <summary>
    /// Count of registered resources
    /// </summary>
    public int ResourceCount => Resources.Count;

    /// <summary>
    /// Count of registered prompts
    /// </summary>
    public int PromptCount => Prompts.Count;

    /// <summary>
    /// The HTTP API caller responsible for executing HTTP requests.
    /// </summary>
    public HttpApiCaller HttpApiCaller { get; private set; }


    /// <summary>
    /// The configuration used for building and managing server-related settings.
    /// </summary>
    public BuilderConfig BuilderConfig { get;  private set; }

    /// <summary>
    /// Creates a new instance of MCPToolCollection
    /// </summary>
    public McpServerInfo(
        string name,
        string description,
        IReadOnlyDictionary<string, ToolInfo> tools,
        IReadOnlyDictionary<string, ResourceInfo> resources,
        IReadOnlyDictionary<string, Prompt> prompts,
        BuilderConfig builderConfig)
    {
        Name = name;
        Description = description;
        Tools = tools;
        Resources = resources;
        Prompts = prompts;
        this.BuilderConfig = builderConfig;
    }

    public void SetHttpCaller(HttpApiCaller httpApiCaller)
    {
        this.HttpApiCaller = httpApiCaller;
    }

    public IEnumerable<McpServerTool> GetMcpTools()
    {
        foreach (var tool in Tools.Values)
        {
            yield return new McpServerApiTool(tool, HttpApiCaller);
        }
    }

   
}