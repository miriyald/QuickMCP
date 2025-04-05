using System.Text.Json.Nodes;

namespace AutoMCP.Helpers
{
    public static class HelperExtensions
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            
            dictionary.Add(key, value);
            return true;
        }
        
        public static JsonObject AsObject(this JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                return obj;
            }
            
            throw new InvalidOperationException("Node is not a JsonObject");
        }
        
        public static JsonArray AsArray(this JsonNode? node)
        {
            if (node is JsonArray array)
            {
                return array;
            }
            
            throw new InvalidOperationException("Node is not a JsonArray");
        }
    }
}
