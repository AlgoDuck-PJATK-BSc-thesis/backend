using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AlgoDuckShared;

public interface IAwsS3Client
{
    public Task<GetObjectResponse> GetDocumentObjectByPathAsync(string path);
    public Task<string> GetDocumentStringByPathAsync(string path);
}

public class AwsS3Client(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) : IAwsS3Client
{
    public async Task<GetObjectResponse> GetDocumentObjectByPathAsync(string path)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = path
        };
        
        var response = await s3Client.GetObjectAsync(getRequest);
        
        return response.HttpStatusCode == HttpStatusCode.OK ? response : throw new AmazonS3Exception($"Could not get document for path {path}");
    }

    public async Task<string> GetDocumentStringByPathAsync(string path)
    {
        var responseObj = await GetDocumentObjectByPathAsync(path);
        
        var buffer = new byte[responseObj.ContentLength];
        var totalBytesRead = 0;

        while (totalBytesRead < responseObj.ContentLength)
        {
            var bytesRead = await responseObj.ResponseStream.ReadAsync(buffer);
            if (bytesRead == 0) break;
            totalBytesRead += bytesRead;
        }

        return Encoding.UTF8.GetString(buffer);
    }
}