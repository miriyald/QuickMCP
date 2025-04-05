using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMCP.Models;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Messages;

namespace AutoMCP;

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(BuilderConfig))]
public partial class AutoMcpJsonSerializerContext:JsonSerializerContext
{
    
}