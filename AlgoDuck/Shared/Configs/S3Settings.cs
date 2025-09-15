namespace AlgoDuck.Shared.Configs;

public sealed class S3Settings
{
    public string Region { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
}