namespace QuickMCP.Types;

/// <summary>
/// Represents metadata details for an authenticator.
/// Includes information such as its name, description, required configuration keys, and type.
/// </summary>
public class AuthenticatorMetadata(
    string name,
    string description,
    List<(string Key, string Description, bool IsRequired)> configKeys,
    string type)
{
    /// <summary>
    /// Gets the name of the authenticator.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description of the authenticator.
    /// Provides details about the authentication method or its purpose.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets the configuration keys required for authentication, including their descriptions and whether they are required.
    /// </summary>
    public List<(string Key, string Description, bool IsRequired)> ConfigKeys { get; } = configKeys;

    /// <summary>
    /// Gets the configuration key used to retrieve authentication settings.
    /// </summary>
    public string Type { get; } = type;
}