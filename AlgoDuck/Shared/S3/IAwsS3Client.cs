using Amazon.S3.Model;

namespace AlgoDuck.Shared.S3;

public interface IAwsS3Client
{
    public Task<string> GetDocumentStringByPathAsync(string path, CancellationToken cancellationToken = default);
    public Task<bool> ObjectExistsAsync(string path, CancellationToken cancellationToken = default);
    public Task PutXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class;
    public Task PostRawFile(IFormFile file, S3BucketType bucketType, CancellationToken cancellationToken = default);
    
}
