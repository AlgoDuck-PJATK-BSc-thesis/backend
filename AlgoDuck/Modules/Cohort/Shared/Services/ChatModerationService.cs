using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class ChatModerationService : IChatModerationService
{
    private readonly HttpClient _httpClient;
    private readonly ChatModerationSettings _settings;
    private readonly ILogger<ChatModerationService> _logger;
    private readonly string _apiKey;

    public ChatModerationService(
        HttpClient httpClient,
        IOptions<ChatModerationSettings> settings,
        ILogger<ChatModerationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        var keyFromEnv = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var keyFromConfig = configuration["OpenAI:ApiKey"];
        _apiKey = string.IsNullOrWhiteSpace(keyFromEnv) ? keyFromConfig ?? string.Empty : keyFromEnv;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }
    }

    public async Task<ChatModerationResult> CheckMessageAsync(
        Guid userId,
        Guid cohortId,
        string content,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return ChatModerationResult.Allowed();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ChatModerationResult.Blocked("Message content cannot be empty.");
        }

        var normalized = NormalizeContent(content);

        try
        {
            var request = BuildRequest(normalized);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "OpenAI moderation request failed with status {StatusCode}: {Body}",
                    response.StatusCode,
                    body);

                return HandleModerationFailure("Moderation service unavailable.");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var moderationResponse = JsonSerializer.Deserialize<ModerationApiResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (moderationResponse == null || moderationResponse.Results == null || moderationResponse.Results.Count == 0)
            {
                return HandleModerationFailure("Moderation response contained no results.");
            }

            var result = moderationResponse.Results[0];

            if (!result.Flagged)
            {
                return ChatModerationResult.Allowed();
            }

            var highest = GetMostSevereCategory(result.CategoryScores);
            var severity = highest.Score;

            if (severity < _settings.SeverityThreshold)
            {
                return ChatModerationResult.Allowed();
            }

            var reason = $"Message was blocked due to {highest.Name} content.";
            return ChatModerationResult.Blocked(reason, highest.Name, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Chat moderation failed for user {UserId} in cohort {CohortId}",
                userId,
                cohortId);

            return HandleModerationFailure("Internal moderation error.");
        }
    }

    private string NormalizeContent(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.Length <= _settings.MaxInputLength)
        {
            return trimmed;
        }

        return trimmed[.._settings.MaxInputLength];
    }

    private ChatModerationResult HandleModerationFailure(string reason)
    {
        if (_settings.FailClosed)
        {
            return ChatModerationResult.Blocked(reason, "internal_error");
        }

        return ChatModerationResult.Allowed();
    }

    private HttpRequestMessage BuildRequest(string content)
    {
        var body = new ModerationApiRequest
        {
            Model = _settings.Model,
            Input = content
        };

        var json = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/moderations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    private static (string Name, double Score) GetMostSevereCategory(ModerationCategoryScores scores)
    {
        var categories = new List<(string Name, double Score)>
        {
            ("hate", scores.Hate),
            ("hate_threatening", scores.HateThreatening),
            ("harassment", scores.Harassment),
            ("harassment_threatening", scores.HarassmentThreatening),
            ("self_harm", scores.SelfHarm),
            ("self_harm_intent", scores.SelfHarmIntent),
            ("self_harm_instructions", scores.SelfHarmInstructions),
            ("sexual", scores.Sexual),
            ("sexual_minors", scores.SexualMinors),
            ("violence", scores.Violence),
            ("violence_graphic", scores.ViolenceGraphic)
        };

        var max = categories[0];
        foreach (var c in categories)
        {
            if (c.Score > max.Score)
            {
                max = c;
            }
        }

        return max;
    }
}