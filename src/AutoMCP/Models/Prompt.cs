namespace AutoMCP.Models
{
    public class Prompt
    {
        public string Name { get; }
        public string Content { get; }
        public string Description { get; }

        public Prompt(string name, string content, string description = "")
        {
            Name = name;
            Content = content;
            Description = description;
        }
    }
}
