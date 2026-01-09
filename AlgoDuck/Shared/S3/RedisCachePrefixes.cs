namespace AlgoDuck.Shared.S3;

public class RedisCachePrefixes
{
    public required string CacheKeyPrefix { get; set; }
    public required string ObjectSuffix { get; set; }
    public required string ExistsSuffix { get; set; }
    public required string KeyEnumerateSuffix { get; set; }
}