using System.Security.Claims;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Shared.S3;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/chat/media")]
[Authorize]
public sealed class UploadMediaEndpoint : ControllerBase
{
    private readonly IUploadMediaHandler _handler;
    private readonly ICohortRepository _cohortRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly ChatMediaSettings _mediaSettings;
    private readonly S3Settings _s3Settings;

    public UploadMediaEndpoint(
        IUploadMediaHandler handler,
        ICohortRepository cohortRepository,
        IAmazonS3 s3Client,
        IOptions<ChatMediaSettings> mediaOptions,
        IOptions<S3Settings> s3Options)
    {
        _handler = handler;
        _cohortRepository = cohortRepository;
        _s3Client = s3Client;
        _mediaSettings = mediaOptions.Value;
        _s3Settings = s3Options.Value;
    }

    [HttpPost]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult<UploadMediaResultDto>> UploadAsync(
        Guid cohortId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue) || !Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var dto = new UploadMediaDto
        {
            CohortId = cohortId,
            File = file
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid cohortId,
        [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue) || !Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        key = (key).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest();
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, cohortId, cancellationToken);
        if (!belongs)
        {
            return Forbid();
        }

        if (!ChatMediaUrl.KeyBelongsToCohort(_mediaSettings, cohortId, key))
        {
            return Forbid();
        }

        var bucketName = string.IsNullOrWhiteSpace(_mediaSettings.BucketName)
            ? _s3Settings.ContentBucketSettings.BucketName
            : _mediaSettings.BucketName;

        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);

            Response.Headers["Cache-Control"] = "private, max-age=3600";

            var contentType = response.Headers.ContentType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(response.ResponseStream, contentType, enableRangeProcessing: true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }
}
