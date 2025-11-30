
namespace AlgoDuck.Shared.S3;



public sealed class S3Settings
{
    public required S3BucketSettings ContentBucketSettings { get; init; }
    public required S3BucketSettings DataBucketSettings { get; init; }
}

public sealed class S3BucketSettings
{
    public required string Region { get; init; }
    public required string BucketName { get; init; }
    public required S3BucketType Type { get; init; }
}

public enum S3BucketType
{
    Content, // available via cloudfront
    Data
}