using System.Text.RegularExpressions;

namespace QuickMCP.Helpers;

public static class StringHelpers
{
    public const int MaxResourceNameLength = 50;
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
        return sanitized.Length > MaxResourceNameLength ? sanitized[..MaxResourceNameLength] : sanitized;
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

    public static string? SanitizeServerName(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        string sanitized = RemoveInvalidPathCharacters(name);
        return ConvertToCamelCase(sanitized);
    }

    private static string RemoveInvalidPathCharacters(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
    }

    private static string ConvertToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var words = name.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var camelCased = string.Concat(words.Select((word, index) =>
            index == 0 ? word.ToLowerInvariant() : char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));

        return camelCased;
    }
}