using System.Net;
using System.Text;
using System.Xml.Serialization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AlgoDuckShared;

public interface IAwsS3Client
{
    public Task<GetObjectResponse> GetDocumentObjectByPathAsync(string path, CancellationToken cancellationToken);
    public Task<string> GetDocumentStringByPathAsync(string path, CancellationToken cancellationToken);
    public Task<bool> ObjectExistsAsync(string path, CancellationToken cancellationToken);
    public Task PutXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken) where T : class;
    
}

public class AwsS3Client(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) : IAwsS3Client
{
    public async Task<GetObjectResponse> GetDocumentObjectByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = s3Settings.Value.BucketName,
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
        if (string.IsNullOrEmpty(s3Settings.Value.BucketName))
        {
            throw new InvalidOperationException("S3 BucketName is not configured");
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = s3Settings.Value.BucketName,
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

    public async Task PutXmlObjectAsync<T>(string path, T obj, CancellationToken cancellationToken = default) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
    
        using var memoryStream = new MemoryStream();
        await using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true))
        {
            serializer.Serialize(writer, obj);
        }
    
        memoryStream.Position = 0;
    
        var putRequest = new PutObjectRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = path,
            InputStream = memoryStream,
            ContentType = "application/xml"
        };
    
        var response = await s3Client.PutObjectAsync(putRequest, cancellationToken);
    
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new AmazonS3Exception($"Could not put XML object at path {path}");
        }
    }
}