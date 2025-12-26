using System.Net;
using System.Text;
using System.Xml.Serialization;
using AlgoDuck.Shared.Http;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Shared.S3;

public class AwsS3Client(
    IAmazonS3 s3Client,
    IOptions<S3Settings> s3Settings
    ) : IAwsS3Client
{
    public async Task<GetObjectResponse> GetDocumentObjectByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = s3Settings.Value.ContentBucketSettings.BucketName,
            Key = path
        };
        
        var response = await s3Client.GetObjectAsync(getRequest, cancellationToken);
        
        return response.HttpStatusCode == HttpStatusCode.OK ? response : throw new AmazonS3Exception($"Could not get document for path {path}");
    }

    public async Task<string> GetDocumentStringByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        var responseObj = await GetDocumentObjectByPathAsync(path, cancellationToken);
        
        var buffer = new byte[responseObj.ContentLength];
        var totalBytesRead = 0;

        while (totalBytesRead < responseObj.ContentLength)
        {
            var bytesRead = await responseObj.ResponseStream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0) break;
            totalBytesRead += bytesRead;
        }

        return Encoding.UTF8.GetString(buffer);
    }

    public async Task<bool> ObjectExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(s3Settings.Value.ContentBucketSettings.BucketName))
        {
            throw new InvalidOperationException("S3 BucketName is not configured");
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = s3Settings.Value.ContentBucketSettings.BucketName,
                Key = path
            };
            
            await s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<Result<T, ErrorObject<string>>> PostXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var memoryStream = new MemoryStream();
        try
        {

            await using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true))
            {
                serializer.Serialize(writer, obj);
            }
    
            memoryStream.Position = 0;

        }
        catch (Exception e)
        {
            return Result<T, ErrorObject<string>>.Err(ErrorObject<string>.InternalError(e.Message));
        }
        
        var putRequest = new PutObjectRequest
        {
            BucketName = s3Settings.Value.ContentBucketSettings.BucketName,
            Key = path,
            InputStream = memoryStream,
            ContentType = "application/xml"
        };
    
        var response = await s3Client.PutObjectAsync(putRequest, cancellationToken);
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            return Result<T, ErrorObject<string>>.Err(response.HttpStatusCode switch
            {
                HttpStatusCode.Forbidden => ErrorObject<string>.Forbidden($"post object failed for {path}"),
                HttpStatusCode.BadRequest => ErrorObject<string>.BadRequest($"post object failed for {path}"),
                _ => ErrorObject<string>.InternalError($"post object failed for {path}")
            });
        }
        return Result<T, ErrorObject<string>>.Ok(obj);
    }

    public async Task PostRawFile(IFormFile file, S3BucketType bucketType = S3BucketType.Content, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = bucketType switch
            {
                S3BucketType.Content => s3Settings.Value.ContentBucketSettings.BucketName,
                S3BucketType.Data => s3Settings.Value.DataBucketSettings.BucketName,
                _ => throw new ArgumentOutOfRangeException(nameof(bucketType), bucketType, null)
            },
            Key = file.FileName,
            InputStream = stream,
            ContentType = file.ContentType
        };
        await s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteDocumentAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = s3Settings.Value.ContentBucketSettings.BucketName,
                Key = path
            }, cancellationToken);
            
            return Result<bool, ErrorObject<string>>.Ok(true);
        }catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("document not found"));
        }
        catch (Exception)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("could not delete"));
        }
    }
}