using System.Text.Json;
using AlgoDuck.Shared.Http;
using StackExchange.Redis;

namespace AlgoDuck.Shared.S3;

public class AwsS3ClientCached(
    IAwsS3Client awsS3Client,
    IDatabase redis
    ) : IAwsS3Client
{

    public async Task<string> GetDocumentStringByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetFromCacheOrInsert($"{path}-object", async () => await awsS3Client.GetDocumentStringByPathAsync(path, cancellationToken)) ?? "";
    }

    public async Task<bool> ObjectExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetFromCacheOrInsert($"{path}-exists", async () => await awsS3Client.ObjectExistsAsync(path, cancellationToken));
    }

    public async Task<Result<T, ErrorObject<string>>> PostXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class
    {
        return await awsS3Client.PostXmlObjectAsync(path, obj, cancellationToken);
    }

    public async Task PostRawFile(IFormFile file, S3BucketType bucketType, CancellationToken cancellationToken = default)
    {
        await awsS3Client.PostRawFile(file, bucketType, cancellationToken);
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteDocumentAsync(string path, CancellationToken cancellationToken = default)
    {
        if ((await redis.StringGetAsync(path)).HasValue)
        {
            await redis.KeyDeleteAsync(path);
        }
        return await awsS3Client.DeleteDocumentAsync(path, cancellationToken);
    }

    private async Task<TResult?> GetFromCacheOrInsert<TResult>(
        string cacheKey,
        Func<Task<TResult>>? insertIfNotFound,
        TimeSpan? expiry = null)
    {
        var redisValue = await redis.StringGetAsync(new RedisKey(cacheKey));
        if (redisValue is not { HasValue: true, IsNullOrEmpty: false })
            return insertIfNotFound != null ? await insertIfNotFound() : default;

        try
        {
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)redisValue.ToString();
            }
            
            return JsonSerializer.Deserialize<TResult>(redisValue.ToString());
        }
        catch (JsonException)
        {
                
        }

        if (insertIfNotFound == null)
            return default;
        
        var result = await insertIfNotFound();

        if (result == null) return result;
        
        try
        {
            var serializedValue = typeof(TResult) == typeof(string)
                ? result.ToString()!
                : JsonSerializer.Serialize(result);
            
            await redis.StringSetAsync(
                new RedisKey(cacheKey), 
                new RedisValue(serializedValue),
                expiry ?? TimeSpan.FromHours(1)
            );
        }
        catch (Exception ex)
        {
            
        }

        return result;
    }
}