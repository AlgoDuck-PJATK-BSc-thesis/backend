namespace AlgoDuck.Shared.Middleware;

public sealed class SecurityHeadersOptions
{
    public string[] ImgOrigins { get; set; } = Array.Empty<string>();
    public string[] ConnectOrigins { get; set; } = Array.Empty<string>();
    public string[] StyleOrigins { get; set; } = Array.Empty<string>();
    public string[] FontOrigins { get; set; } = Array.Empty<string>();
    public bool ReportOnly { get; set; }
}