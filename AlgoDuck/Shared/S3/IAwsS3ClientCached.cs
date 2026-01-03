using System.Text.Json;
using AlgoDuck.Shared.Http;
using StackExchange.Redis;

namespace AlgoDuck.Shared.S3;

public class AwsS3ClientCached(
    IAwsS3Client awsS3Client,
    IDatabase redis,
    ILogger<AwsS3ClientCached> logger
) : IAwsS3Client
{
    private const string CacheKeyPrefix = "s3cache:";
    private const string ObjectSuffix = ":object";
    private const string ExistsSuffix = ":exists";
    
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(1);

    public async Task<Result<string, ErrorObject<string>>> GetDocumentStringByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(path, ObjectSuffix);

        var cachedValue = await TryGetFromCacheAsync<string>(cacheKey);
        if (cachedValue.HasValue)
        {
            return Result<string, ErrorObject<string>>.Ok(cachedValue.Value!);
        }

        var result = await awsS3Client.GetDocumentStringByPathAsync(path, cancellationToken);

        if (result.IsErr)
            return result;
        
        await SetCacheAsync(cacheKey, result.AsOk);
        await SetCacheAsync(BuildCacheKey(path, ExistsSuffix), true);

        return result;
    }

    public async Task<Result<T, ErrorObject<string>>> GetJsonObjectByPathAsync<T>(string path, CancellationToken cancellationToken = default) where T : class
    {
        
        var cacheKey = BuildCacheKey(path, ObjectSuffix);
        var cachedValue = await TryGetFromCacheAsync<T>(cacheKey);
        
        if (cachedValue.HasValue)
        {
            return Result<T, ErrorObject<string>>.Ok(cachedValue.Value!);
        }

        var result = await awsS3Client.GetJsonObjectByPathAsync<T>(path, cancellationToken);


        if (result.IsErr)
            return result;
        
        await SetCacheAsync(cacheKey, result.AsOk);
        await SetCacheAsync(BuildCacheKey(path, ExistsSuffix), true);

        return result;
    }

    public async Task<Result<T, ErrorObject<string>>> GetXmlObjectByPathAsync<T>(string path, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = BuildCacheKey(path, ObjectSuffix);

        var cachedValue = await TryGetFromCacheAsync<T>(cacheKey);
        if (cachedValue.HasValue)
        {
            return Result<T, ErrorObject<string>>.Ok(cachedValue.Value!);
        }

        var result = await awsS3Client.GetXmlObjectByPathAsync<T>(path, cancellationToken);

        if (result.IsErr)
            return result;
        
        await SetCacheAsync(cacheKey, result.AsOk!);
        await SetCacheAsync(BuildCacheKey(path, ExistsSuffix), true);

        return result;
    }

    public async Task<Result<bool, ErrorObject<string>>> ObjectExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(path, ExistsSuffix);

        var cachedValue = await TryGetFromCacheAsync<bool>(cacheKey);
        if (cachedValue.HasValue)
        {
            return Result<bool, ErrorObject<string>>.Ok(cachedValue.Value);
        }

        var result = await awsS3Client.ObjectExistsAsync(path, cancellationToken);

        if (result.IsOk)
        {
            await SetCacheAsync(cacheKey, result.AsOk);
        }

        return result;
    }

    public async Task<Result<T, ErrorObject<string>>> PostXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class
    {
        var result = await awsS3Client.PostXmlObjectAsync(path, obj, cancellationToken);

        if (result.IsErr)
            return result;
        
        await InvalidateCacheForPathAsync(path);
        await SetCacheAsync(BuildCacheKey(path, ObjectSuffix), obj);
        await SetCacheAsync(BuildCacheKey(path, ExistsSuffix), true);

        return result;
    }

    public async Task<Result<T, ErrorObject<string>>> PostJsonObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class
    {
        var result = await awsS3Client.PostJsonObjectAsync(path, obj, cancellationToken);

        if (result.IsErr)
            return result;
        
        await InvalidateCacheForPathAsync(path);
        await SetCacheAsync(BuildCacheKey(path, ObjectSuffix), obj);
        await SetCacheAsync(BuildCacheKey(path, ExistsSuffix), true);

        return result;
    }

    public async Task<Result<bool, ErrorObject<string>>> PostRawFileAsync(IFormFile file, S3BucketType bucketType = S3BucketType.Content, CancellationToken cancellationToken = default)
    {
        var result = await awsS3Client.PostRawFileAsync(file, bucketType, cancellationToken);

        if (result.IsOk)
        {
            await InvalidateCacheForPathAsync(file.FileName);
        }

        return result;
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteDocumentAsync(string path, CancellationToken cancellationToken = default)
    {
        var result = await awsS3Client.DeleteDocumentAsync(path, cancellationToken);

        await InvalidateCacheForPathAsync(path);

        return result;
    }

    private static string BuildCacheKey(string path, string suffix) => $"{CacheKeyPrefix}{path}{suffix}";

    private async Task InvalidateCacheForPathAsync(string path)
    {
        try
        {
            var keysToDelete = new RedisKey[]
            {
                new(BuildCacheKey(path, ObjectSuffix)),
                new(BuildCacheKey(path, ExistsSuffix))
            };

            await redis.KeyDeleteAsync(keysToDelete);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to invalidate cache for path {Path}", path);
        }
    }

    private async Task<CacheResult<T>> TryGetFromCacheAsync<T>(string cacheKey)
    {
        try
        {
            var redisValue = await redis.StringGetAsync(new RedisKey(cacheKey));

            if (!redisValue.HasValue || redisValue.IsNullOrEmpty)
            {
                return CacheResult<T>.Miss();
            }

            var stringValue = redisValue.ToString();

            if (typeof(T) == typeof(string))
            {
                return CacheResult<T>.Hit((T)(object)stringValue);
            }

            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(stringValue, out var boolValue))
                {
                    return CacheResult<T>.Hit((T)(object)boolValue);
                }
                return CacheResult<T>.Miss();
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(stringValue);
            
            return deserializedValue != null
                ? CacheResult<T>.Hit(deserializedValue)
                : CacheResult<T>.Miss();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize cached value for key {CacheKey}", cacheKey);
            await redis.KeyDeleteAsync(new RedisKey(cacheKey));
            return CacheResult<T>.Miss();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve from cache for key {CacheKey}", cacheKey);
            return CacheResult<T>.Miss();
        }
    }

    private async Task SetCacheAsync<T>(string cacheKey, T value, TimeSpan? expiry = null)
    {
        try
        {
            string serializedValue;

            if (typeof(T) == typeof(string))
            {
                serializedValue = value?.ToString() ?? string.Empty;
            }
            else if (typeof(T) == typeof(bool))
            {
                serializedValue = value?.ToString()?.ToLowerInvariant() ?? "false";
            }
            else
            {
                serializedValue = JsonSerializer.Serialize(value);
            }

            await redis.StringSetAsync(
                new RedisKey(cacheKey),
                new RedisValue(serializedValue),
                expiry ?? DefaultExpiry
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set cache for key {CacheKey}", cacheKey);
        }
    }

    private readonly struct CacheResult<T>
    {
        public bool HasValue { get; }
        public T? Value { get; }

        private CacheResult(bool hasValue, T? value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public static CacheResult<T> Hit(T value) => new(true, value);
        public static CacheResult<T> Miss() => new(false, default);
    }
}