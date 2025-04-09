using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.Build;

public class BuildSpecCommandSettings:BuildCommandSettings
{
    [Description("The URL to the api documentation to create the openapi spec from")]
    [CommandOption("-d|--documentation-url <URL>")]
    public string? DocumentationUrl { get; set; }
    
    
    [Description("The firecrawl api key to use for scraping the documentation")]
    [CommandOption("-f|--firecrawl-api-key <KEY>")]
    public string? FirecrawlApiKey { get; set; }
    
    [Description("The Google Gemini api key to convert the documentation to openapi specs")]
    [CommandOption("-k|--google-api-key <KEY>")]
    public string? GoogleApiKey { get; set; }
    
    [Description("The output file to write the openapi spec to")]
    [CommandOption("-o|--output-file <FILE>")]
    public string? OutputFile { get; set; }

    public override ValidationResult Validate()
    {
        if(string.IsNullOrWhiteSpace(DocumentationUrl))
            return ValidationResult.Error("Documentation url is required");
        

        if (string.IsNullOrEmpty(GoogleApiKey))
        {
            GoogleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if(string.IsNullOrWhiteSpace(GoogleApiKey))
                return ValidationResult.Error("Please provide a google api key or set the GOOGLE_API_KEY environment variable");
        }
        
        if (string.IsNullOrEmpty(FirecrawlApiKey))
        {
            FirecrawlApiKey = Environment.GetEnvironmentVariable("FIRECRAWL_API_KEY");
            if(string.IsNullOrWhiteSpace(FirecrawlApiKey))
                return ValidationResult.Error("Please provide a firecrawl api key or set the FIRECRAWL_API_KEY environment variable");
            
        }
        return base.Validate();
    }
}