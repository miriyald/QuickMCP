using System.Text.Json.Nodes;

namespace AutoMCP.Models
{
    public class MCPResource
    {
        public string Name { get; }
        public JsonNode Schema { get; }
        public string Description { get; }
        public string Uri { get; }

        public MCPResource(string name, JsonNode schema, string description)
        {
            Name = name;
            Schema = schema;
            Description = description;
            Uri = $"/resource/{name}";
        }
    }
}
