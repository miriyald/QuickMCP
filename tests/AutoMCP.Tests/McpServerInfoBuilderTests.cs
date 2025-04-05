using AutoMCP.Builders;
using Shouldly;

namespace AutoMCP.Tests;

public class McpServerInfoBuilderTests
{
    [Theory]
    [InlineData("ai21.json")]
    [InlineData("anthropic.yaml")]
    [InlineData("assemblyai.yaml")]
    [InlineData("cohere.yaml")]
    [InlineData("dedoose.json")]
    [InlineData("github.yaml")]
    [InlineData("huggingface.yaml")]
    [InlineData("ipinfo.yaml")]
    [InlineData("langsmith.json")]
    [InlineData("leonardo.yaml")]
    [InlineData("mystic.yaml")]
    [InlineData("ollama.yaml")]
    [InlineData("openai.yaml")]
    [InlineData("petstore.yaml")]
    [InlineData("replicate.json")]
    [InlineData("special-cases.yaml")]
    [InlineData("together.yaml")]
    [InlineData("twitch.json")]
    [InlineData("filtering.yaml")]
    [InlineData("heygen.yaml")]
    [InlineData("instill.yaml")]
    [InlineData("ideogram.yaml")]
    [InlineData("google-gemini.yaml")]
    [InlineData("mistral.yaml")]
    [InlineData("elevenlabs.json")]
    [InlineData("jina.json")]
    [InlineData("runway.yaml")]
    [InlineData("recraft.yaml")]
    [InlineData("luma.yaml")]
    public async Task OpenApiTest(string fileName)
    {
        var path = $"specs/{fileName}";
        var serverInfo = await McpServerBuilder.ForOpenApi().FromFile(path).BuildAsync();
        // Assert the builder contains some tools
        serverInfo.Tools.Count.ShouldBeGreaterThan(0);

        foreach (var tool in serverInfo.Tools)
        {
            // Assert that the key exists and is not empty
            tool.Key.ShouldNotBeNullOrEmpty();

            // Assert simple properties are not null or empty
            tool.Value.Method.ShouldNotBeNullOrEmpty();
            tool.Value.Url.ShouldNotBeNullOrEmpty();
            tool.Value.ContentType.ShouldNotBeNullOrEmpty();

            // Assert metadata properties using ShouldSatisfyAllConditions
            tool.Value.Metadata.ShouldSatisfyAllConditions(
                () => tool.Value.Metadata.Name.ShouldNotBeNullOrEmpty(),
                () => tool.Value.Metadata.Description.ShouldNotBeNullOrEmpty(),
                // Depending on the type, you may need to assert differently (e.g., empty JSON string is valid)
                () => tool.Value.Metadata.InputSchema.ShouldNotBeNull(),
                () => tool.Value.Metadata.Parameters.ShouldNotBeNull(),
                () => tool.Value.Metadata.Tags.ShouldNotBeNull(),
                () => tool.Value.Metadata.ServerInfo.ShouldNotBeNull(),
                () => tool.Value.Metadata.ServerInfo?.Name.ShouldNotBeNullOrEmpty(),
                () => tool.Value.Metadata.ResponseSchema.ShouldNotBeNull()
            );

            // If you need to further assert collections like Parameters or Tags, you can do so:
            //tool.Value.Metadata.Parameters.Count().ShouldBeGreaterThan(0);
            tool.Value.Metadata.Tags.Count().ShouldBeGreaterThan(0);
        }

        foreach (var tool in serverInfo.Tools)
        {
            Console.WriteLine(tool.Key);
            Console.WriteLine($"Method: {tool.Value.Method}");
            Console.WriteLine($"Url: {tool.Value.Url}");
            Console.WriteLine($"ContentType: {tool.Value.ContentType}");
            Console.WriteLine($"Metadata.Name: {tool.Value.Metadata.Name}");
            Console.WriteLine($"Metadata.Description: {tool.Value.Metadata.Description}");
            Console.WriteLine($"Metadata.InputSchema: {tool.Value.Metadata.InputSchema}");
            Console.WriteLine(
                $"Metadata.Parameters: {string.Join(", ", tool.Value.Metadata.Parameters.Select(p => p.ToString()))}");
            Console.WriteLine($"Metadata.Tags: {string.Join(", ", tool.Value.Metadata.Tags)}");
            Console.WriteLine($"Metadata.ServerInfo: {tool.Value.Metadata.ServerInfo}");
            Console.WriteLine($"Metadata.ResponseSchema: {tool.Value.Metadata.ResponseSchema}");
        }
    }
}