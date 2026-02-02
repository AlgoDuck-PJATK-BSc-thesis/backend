using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using AlgoDuck.Shared.Http;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Shared.S3;

public class AwsS3Client : IAwsS3Client
{
    private readonly IAmazonS3 _s3Client;
    private readonly IOptions<S3Settings> _s3Settings;

    public AwsS3Client(IOptions<S3Settings> s3Settings, IAmazonS3 s3Client)
    {
        _s3Settings = s3Settings;
        _s3Client = s3Client;
    }

    public async Task<Result<string, ErrorObject<string>>> GetDocumentStringByPathAsync(string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _s3Settings.Value.DataBucketSettings.BucketName,
                Key = path
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return Result<string, ErrorObject<string>>.Err(
                    ErrorObject<string>.InternalError($"Could not get document for path {path}"));
            }

            var buffer = new byte[response.ContentLength];
            var totalBytesRead = 0;

            while (totalBytesRead < response.ContentLength)
            {
                var bytesRead = await response.ResponseStream.ReadAsync(
                    buffer.AsMemory(totalBytesRead),
                    cancellationToken
                );
                if (bytesRead == 0) break;
                totalBytesRead += bytesRead;
            }

            return Result<string, ErrorObject<string>>.Ok(Encoding.UTF8.GetString(buffer));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result<string, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"Document not found at path {path}"));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Result<string, ErrorObject<string>>.Err(
                ErrorObject<string>.Forbidden($"Access denied for path {path}"));
        }
        catch (Exception ex)
        {
            return Result<string, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Error retrieving document: {ex.Message}"));
        }
    }

    public async Task<Result<T, ErrorObject<string>>> GetJsonObjectByPathAsync<T>(string path,
        CancellationToken cancellationToken = default) where T : class
    {
        var stringResult = await GetDocumentStringByPathAsync(path, cancellationToken);
        if (stringResult.IsErr)
            return Result<T, ErrorObject<string>>.Err(stringResult.AsT1);

        try
        {
            var result = JsonSerializer.Deserialize<T>(stringResult.AsT0);
            if (result == null)
            {
                return Result<T, ErrorObject<string>>.Err(
                    ErrorObject<string>.InternalError($"Failed to deserialize JSON at path {path}"));
            }

            return Result<T, ErrorObject<string>>.Ok(result);
        }
        catch (JsonException ex)
        {
            return Result<T, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest($"Invalid JSON format at path {path}: {ex.Message}"));
        }
    }

    public async Task<Result<T, ErrorObject<string>>> GetXmlObjectByPathAsync<T>(string path,
        CancellationToken cancellationToken = default) where T : class
    {
        var stringResult = await GetDocumentStringByPathAsync(path, cancellationToken);

        if (stringResult.IsErr)
            return Result<T, ErrorObject<string>>.Err(stringResult.AsT1);

        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(stringResult.AsT0);
            var obj = serializer.Deserialize(reader) as T;

            if (obj == null)
            {
                return Result<T, ErrorObject<string>>.Err(
                    ErrorObject<string>.InternalError($"Failed to deserialize XML at path {path}"));
            }

            return Result<T, ErrorObject<string>>.Ok(obj);
        }
        catch (InvalidOperationException ex)
        {
            return Result<T, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest($"Invalid XML format at path {path}: {ex.Message}"));
        }
    }

    public async Task<Result<bool, ErrorObject<string>>> ObjectExistsAsync(string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_s3Settings.Value.DataBucketSettings.BucketName))
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError("S3 BucketName is not configured"));
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _s3Settings.Value.DataBucketSettings.BucketName,
                Key = path
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return Result<bool, ErrorObject<string>>.Ok(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result<bool, ErrorObject<string>>.Ok(false);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.Forbidden($"Access denied for path {path}"));
        }
        catch (Exception ex)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Error checking object existence: {ex.Message}"));
        }
    }

    public async Task<Result<T, ErrorObject<string>>> PostXmlObjectAsync<T>(string path, T obj,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var memoryStream = new MemoryStream();

            await using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true))
            {
                serializer.Serialize(writer, obj);
                await writer.WriteLineAsync();
                await writer.FlushAsync(cancellationToken);
            }

            memoryStream.Position = 0;

            var putRequest = new PutObjectRequest
            {
                BucketName = _s3Settings.Value.DataBucketSettings.BucketName,
                Key = path,
                InputStream = memoryStream,
                ContentType = "application/xml"
            };

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return Result<T, ErrorObject<string>>.Err(response.HttpStatusCode switch
                {
                    HttpStatusCode.Forbidden => ErrorObject<string>.Forbidden($"Post object failed for {path}"),
                    HttpStatusCode.BadRequest => ErrorObject<string>.BadRequest($"Post object failed for {path}"),
                    _ => ErrorObject<string>.InternalError($"Post object failed for {path}")
                });
            }

            return Result<T, ErrorObject<string>>.Ok(obj);
        }
        catch (Exception ex)
        {
            return Result<T, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Error posting XML object: {ex.Message}"));
        }
    }

    public async Task<Result<T, ErrorObject<string>>> PostJsonObjectAsync<T>(string path, T obj,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
            using var stream = new MemoryStream(jsonBytes);

            var response = await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _s3Settings.Value.DataBucketSettings.BucketName,
                Key = path,
                InputStream = stream,
                ContentType = "application/json"
            }, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return Result<T, ErrorObject<string>>.Err(response.HttpStatusCode switch
                {
                    HttpStatusCode.Forbidden => ErrorObject<string>.Forbidden($"Post object failed for {path}"),
                    HttpStatusCode.BadRequest => ErrorObject<string>.BadRequest($"Post object failed for {path}"),
                    _ => ErrorObject<string>.InternalError($"Post object failed for {path}")
                });
            }

            return Result<T, ErrorObject<string>>.Ok(obj);
        }
        catch (Exception ex)
        {
            return Result<T, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Error posting JSON object: {ex.Message}"));
        }
    }

    public async Task<Result<bool, ErrorObject<string>>> PostRawFileAsync(string path, Stream fileContents,
        string? contentType = null, S3BucketType bucketType = S3BucketType.Content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketType switch
                {
                    S3BucketType.Content => _s3Settings.Value.ContentBucketSettings.BucketName,
                    S3BucketType.Data => _s3Settings.Value.DataBucketSettings.BucketName,
                    _ => throw new ArgumentOutOfRangeException(nameof(bucketType), bucketType, null)
                },
                Key = path,
                InputStream = fileContents,
                ContentType = contentType ?? "application/octet-stream"
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return Result<bool, ErrorObject<string>>.Err(response.HttpStatusCode switch
                {
                    HttpStatusCode.Forbidden => ErrorObject<string>.Forbidden($"Post file failed for {path}"),
                    HttpStatusCode.BadRequest => ErrorObject<string>.BadRequest($"Post file failed for {path}"),
                    _ => ErrorObject<string>.InternalError($"Post file failed for {path}")
                });
            }

            return Result<bool, ErrorObject<string>>.Ok(true);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest($"Invalid bucket type: {bucketType}"));
        }
        catch (Exception ex)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Error posting file: {ex.Message}"));
        }
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteDocumentAsync(string path,
        S3BucketType bucketType = S3BucketType.Content, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _s3Settings.Value.DataBucketSettings.BucketName,
                Key = path
            }, cancellationToken);

            return Result<bool, ErrorObject<string>>.Ok(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound("Document not found"));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.Forbidden($"Access denied for path {path}"));
        }
        catch (Exception ex)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"Could not delete: {ex.Message}"));
        }
    }


    public async Task<Result<ICollection<string>, ErrorObject<string>>> DeleteAllByPrefixAsync(string prefix,
        S3BucketType bucketType = S3BucketType.Data, CancellationToken cancellationToken = default)
    {
        var bucketName = _s3Settings.Value.DataBucketSettings.BucketName;

        var objects = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = prefix,
        }, cancellationToken);

        var deletedVersions = new Dictionary<string, string>();

        try
        {
            foreach (var obj in objects.S3Objects)
            {
                var deleteResponse = await _s3Client.DeleteObjectAsync(bucketName, obj.Key, cancellationToken);
                deletedVersions[obj.Key] = deleteResponse.VersionId;
            }

            return Result<ICollection<string>, ErrorObject<string>>.Ok(deletedVersions.Select(kv => kv.Key).ToList());
        }
        catch
        {
            foreach (var (key, versionId) in deletedVersions)
            {
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    VersionId = versionId
                }, cancellationToken);
            }

            return Result<ICollection<string>, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError($"failed deleting {prefix}"));
        }
    }

    private string ExtractBucketName(S3BucketType? bucketType = null)
    {
        return bucketType switch
        {
            S3BucketType.Content => _s3Settings.Value.ContentBucketSettings.BucketName,
            S3BucketType.Data => _s3Settings.Value.DataBucketSettings.BucketName,
            null => _s3Settings.Value.DataBucketSettings.BucketName,
            _ => throw new ArgumentOutOfRangeException(nameof(bucketType), bucketType, null)
        };
    }
}