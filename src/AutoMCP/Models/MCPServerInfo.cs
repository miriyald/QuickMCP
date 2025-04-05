namespace AutoMCP.Models
{
    /// <summary>
    /// Collection of MCP tools, resources, and prompts
    /// </summary>
    public class MCPServerInfo
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
        /// Creates a new instance of MCPToolCollection
        /// </summary>
        public MCPServerInfo(
            string name,
            string description,
            IReadOnlyDictionary<string, ToolInfo> tools,
            IReadOnlyDictionary<string, ResourceInfo> resources,
            IReadOnlyDictionary<string, Prompt> prompts)
        {
            Name = name;
            Description = description;
            Tools = tools;
            Resources = resources;
            Prompts = prompts;
        }
        
        // /// <summary>
        // /// Creates a new instance of MCPToolCollection from an MCPServer
        // /// </summary>
        // public static MCPToolCollection FromServer(
        //     MCPServer server,
        //     Dictionary<string, Prompt> prompts = null)
        // {
        //     return new MCPToolCollection(
        //         server.ServerName,
        //         "Tools generated from OpenAPI specification",
        //         server.RegisteredTools,
        //         server.RegisteredResources,
        //         prompts ?? new Dictionary<string, Prompt>()
        //     );
        // }
        
        // /// <summary>
        // /// Registers all tools, resources, and prompts with an MCPServer
        // /// </summary>
        // public void RegisterWithServer(FastMCP fastMcp)
        // {
        //     // Register tools
        //     foreach (var (name, toolInfo) in Tools)
        //     {
        //         fastMcp.AddTool(name, toolInfo.Metadata["description"].ToString(), toolInfo.Function);
        //     }
        //     
        //     // Register resources
        //     foreach (var (name, resourceInfo) in Resources)
        //     {
        //         var resourceName = name;
        //         var resourceSchema = resourceInfo.Schema;
        //         var resourceDescription = resourceInfo.Metadata["description"].ToString();
        //         
        //         var resource = new MCPResource(resourceName, resourceSchema, resourceDescription);
        //         fastMcp.AddResource(resource);
        //     }
        //     
        //     // Register prompts
        //     foreach (var (name, prompt) in Prompts)
        //     {
        //         fastMcp.AddPrompt(prompt);
        //     }
        // }
    }
}