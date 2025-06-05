using System.ComponentModel;
using QuickMCP.Types;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.Build;

public class BuildConfigCommandSettings : BuildCommandSettings
{
    [Description("API Specification type OpenAPI (openapi) or Google discovery (discovery).")]
    [CommandOption("-t|--api-type <API_TYPE>")]
    public string ApiType { get; set; } = "openapi";

    [Description("The URL of the OpenAPI or Google Discovery specification.")]
    [CommandOption("-s|--spec-url <SPEC_URL>")]
    public string? SpecUrl { get; set; }

    [Description("The Path of the OpenAPI or Google Discovery specification.")]
    [CommandOption("-p|--spec-path <SPEC_PATH>")]
    public string? SpecPath { get; set; }

    [Description("The name of the server to use.")]
    [CommandOption("-n|--server-name <SERVER_NAME>")]
    public string? ServerName { get; set; }

    [Description("Base URL for the API.")]
    [CommandOption("-a|--api-base-url <API_BASE_URL>")]
    public string? ApiBaseUrl { get; set; }

    [Description("The type of authentication to use.")]
    [CommandOption("--auth|--auth-type <AUTH_TYPE>")]
    public string? AuthType { get; set; }

    [Description("Exclude api paths from the generated client.")]
    [CommandOption("-x|--exclude-paths <EXCLUDE_PATHS>")]
    public string[]? ExcludePaths { get; set; }

    [Description("Only generate api paths for the specified paths.")]
    [CommandOption("-f|--only-for-paths <ONLY_FOR_PATHS>")]
    public string[]? OnlyForPaths { get; set; }

    [Description("Output directory path for the generated client.")]
    [CommandOption("-o|--output-path <OUTPUT_PATH>")]
    public string? OutputPath { get; set; }

    [Description("Generate AI metadata for the methods and parameters.")]
    [CommandOption("-m|--ai-metadata")]
    public bool? AiMetadata { get; set; }

    [Description("Google Gemini API Key for the metadata generation.")]
    [CommandOption("-k|--ai-api-key <AI_API_KEY>")]
    public string? AiApiKey { get; set; }

    [Description("Skip the authentication configuration at the end of build.")]
    [CommandOption("--skip-auth-config")]
    public bool? SkipAuthConfig { get; set; }
    
    
    [Description("Host Protocol.STDIO or HTTP.")]
    [CommandOption("-h|--host-protocol")]
    public string? HostProtocol { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(SpecUrl) && string.IsNullOrEmpty(SpecPath))
            return ValidationResult.Error("You must specify a specification file or url.");
        if (AiMetadata == true)
        {
            if (string.IsNullOrEmpty(AiApiKey))
                this.AiApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrEmpty(AiApiKey))
                return ValidationResult.Error(
                    "You must specify an Google Gemini API Key or GOOGLE_API_KEY environment variable for the metadata generation.");
        }

        return base.Validate();
    }

    public BuilderConfig GetBuilderConfig()
    {
        return new BuilderConfig()
        {
            Type = this.ApiType ?? "openapi",
            ApiBaseUrl = ApiBaseUrl,
            ApiSpecPath = SpecPath,
            ServerName = ServerName,
            ApiSpecUrl = SpecUrl,
            HostProtocol = HostProtocol,
            ExcludedPaths = this.ExcludePaths?.SelectMany(s => s.Split(',')).ToList(),
            IncludedPaths = this.OnlyForPaths?.SelectMany(s => s.Split(',')).ToList()
        };
    }
}