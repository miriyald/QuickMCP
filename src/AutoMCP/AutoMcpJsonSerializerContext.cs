using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMCP.Helpers;
using AutoMCP.Types;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Messages;

namespace AutoMCP;

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(BuilderConfig))]
[JsonSerializable(typeof(McpServerInfo))]
[JsonSerializable(typeof(ToolInfo))]
[JsonSerializable(typeof(List<ToolInfo>))]
[JsonSerializable(typeof(AuthConfig))]
[JsonSerializable(typeof(Dictionary<string,string>))]
[JsonSerializable(typeof(OperationInfo))]
public partial class AutoMcpJsonSerializerContext:JsonSerializerContext
{
    
}