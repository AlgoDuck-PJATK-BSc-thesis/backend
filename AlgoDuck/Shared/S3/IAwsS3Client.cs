using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlgoDuck.Shared.S3;

public interface IAwsS3Client
{
    Task<Result<string, ErrorObject<string>>> GetDocumentStringByPathAsync(string path,
        CancellationToken cancellationToken = default);

    Task<Result<T, ErrorObject<string>>> GetJsonObjectByPathAsync<T>(string path,
        CancellationToken cancellationToken = default) where T : class;

    Task<Result<T, ErrorObject<string>>> GetXmlObjectByPathAsync<T>(string path,
        CancellationToken cancellationToken = default) where T : class;

    Task<Result<bool, ErrorObject<string>>> ObjectExistsAsync(string path,
        CancellationToken cancellationToken = default);

    Task<Result<T, ErrorObject<string>>> PostXmlObjectAsync<T>(string path, T obj,
        CancellationToken cancellationToken = default) where T : class;

    Task<Result<T, ErrorObject<string>>> PostJsonObjectAsync<T>(string path, T obj,
        CancellationToken cancellationToken = default) where T : class;


    Task<Result<bool, ErrorObject<string>>> PostRawFileAsync(string path, Stream fileContents,
        string? contentType = null,
        S3BucketType bucketType = S3BucketType.Content,
        CancellationToken cancellationToken = default);

    Task<Result<bool, ErrorObject<string>>> DeleteDocumentAsync(string path,
        S3BucketType bucketType = S3BucketType.Data, CancellationToken cancellationToken = default);


    Task<Result<ICollection<string>, ErrorObject<string>>> DeleteAllByPrefixAsync(string prefix,
        S3BucketType bucketType = S3BucketType.Data, CancellationToken cancellationToken = default);
}