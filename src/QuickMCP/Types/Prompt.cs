namespace QuickMCP.Types;

/// <summary>
/// Represents a prompt containing a name, content, and an optional description.
/// </summary>
public class Prompt
{
    /// <summary>
    /// Gets the name of the prompt.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the content of the prompt.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the description of the prompt.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Prompt"/> class with the specified name, content, and description.
    /// </summary>
    /// <param name="name">The name of the prompt.</param>
    /// <param name="content">The content of the prompt.</param>
    /// <param name="description">The optional description of the prompt. Defaults to an empty string.</param>
    public Prompt(string name, string content, string description = "")
    {
        Name = name;
        Content = content;
        Description = description;
    }
}