using System.Text.RegularExpressions;

namespace AutoMCP.Helpers;

public static class StringHelpers
{
    public static string SingularizeResource(string resource)
    {
        if (resource.EndsWith("ies"))
        {
            return resource[..^3] + "y";
        }
        else if (resource.EndsWith("sses"))
        {
            return resource;
        }
        else if (resource.EndsWith("s") && !resource.EndsWith("ss"))
        {
            return resource[..^1];
        }
        return resource;
    }

    public static string SanitizeName(string name)
    {
        var sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_-]", "_");
        return sanitized.Length > 64 ? sanitized[..64] : sanitized;
    }

    public static string SanitizeToolName(string name, string? serverPrefix = null)
    {
        if (!string.IsNullOrEmpty(serverPrefix))
        {
            var prefixedName = $"{serverPrefix}_{name}";
            return SanitizeName(prefixedName);
        }
        return SanitizeName(name);
    }
}