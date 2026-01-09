using System.Text.Json.Serialization;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationResponseStatus
{
    Queued,
    Pending,
    Succeeded,
    Failed
}