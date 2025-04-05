namespace AutoMCP.Models;

public class PreparedRequest
{
    public string Url { get; }
    public Dictionary<string, string> Parameters { get; }
    public Dictionary<string, string> Headers { get; }
    public object? Body { get; }
    public bool DryRun { get; }

    public PreparedRequest(
        string url,
        Dictionary<string, string> parameters,
        Dictionary<string, string> headers,
        object? body,
        bool dryRun)
    {
        Url = url;
        Parameters = parameters;
        Headers = headers;
        Body = body;
        DryRun = dryRun;
    }

    public void Deconstruct(
        out string url,
        out Dictionary<string, string> parameters,
        out Dictionary<string, string> headers,
        out object? body,
        out bool dryRun)
    {
        url = Url;
        parameters = Parameters;
        headers = Headers;
        body = Body;
        dryRun = DryRun;
    }
}