using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantRepository
{
    public Task<AssistantChat?> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default);

    public Task<AssistanceMessage> CreateNewChatMessage(ChatMessageInsertDto chatMessage,
        CancellationToken cancellationToken = default);
}

public class AssistantRepository(
    ApplicationCommandDbContext dbContext,
    IAwsS3Client awsS3Client
) : IAssistantRepository
{
    public Task<AssistantChat?> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return dbContext.AssistantChats
            .Include(e => e.Messages.OrderByDescending(ie => ie.CreatedOn).Take(10)
            ).ThenInclude(e => e.Fragments)
            .FirstOrDefaultAsync(
                e => e.UserId == request.UserId && e.ProblemId == request.ExerciseId && e.Name == request.ChatName,
                cancellationToken);
    }

    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var problemTemplate = await GetTemplateAsync(problemId, cancellationToken);
        var testCases = await GetTestCasesAsync(problemId, cancellationToken);
        var problemInfos = await GetProblemInfoAsync(problemId, cancellationToken);

        var problem = await dbContext.Problems
                          .Include(p => p.Category)
                          .Include(p => p.Difficulty)
                          .FirstOrDefaultAsync(p => p.ProblemId == problemId, cancellationToken: cancellationToken)
                      ?? throw new NotFoundException($"Problem {problemId} not found");

        return new ProblemDto
        {
            Description = problemInfos.Description,
            ProblemId = problem.ProblemId,
            TemplateContents = problemTemplate.Template,
            Title = problemInfos.Title,
            TestCases = testCases.Select(t => new TestCaseDto
            {
                Display = t.IsPublic ? t.Display : "",
                DisplayRes = t.IsPublic ? t.DisplayRes : "",
                IsPublic = t.IsPublic,
                TestCaseId = t.TestCaseId,
            }),
            Category = new CategoryDto
            {
                Name = problem.Category.CategoryName
            },
            Difficulty = new DifficultyDto
            {
                Name = problem.Difficulty.DifficultyName
            }
        };
    }

    private async Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        return XmlToObjectParser.ParseXmlString<ProblemS3PartialTemplate>(
            await awsS3Client.GetDocumentStringByPathAsync($"problems/{exerciseId}/template.xml", cancellationToken)
        ) ?? throw new XmlParsingException();
    }

    private async Task<ProblemS3PartialInfo> GetProblemInfoAsync(Guid problemId,
        CancellationToken cancellation = default, SupportedLanguage lang = SupportedLanguage.En)
    {
        var objectPath = $"problems/{problemId}/infos/{lang.GetDisplayName().ToLowerInvariant()}.xml";
        if (!await awsS3Client.ObjectExistsAsync(objectPath, cancellation))
        {
            throw new NotFoundException(objectPath);
        }

        var problemInfosRaw = await awsS3Client.GetDocumentStringByPathAsync(objectPath, cancellation);
        var problemInfos = XmlToObjectParser.ParseXmlString<ProblemS3PartialInfo>(problemInfosRaw)
                           ?? throw new XmlParsingException(objectPath);

        return problemInfos;
    }

    private async Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var exerciseDbPartialTestCases =
            await dbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId)
                .ToDictionaryAsync(t => t.TestCaseId, t => t, cancellationToken: cancellationToken);

        var exerciseS3PartialTestCases = XmlToObjectParser.ParseXmlString<TestCaseS3WrapperObject>(
                                             await awsS3Client.GetDocumentStringByPathAsync(
                                                 $"problems/{exerciseId}/test-cases.xml", cancellationToken))
                                         ?? throw new XmlParsingException($"problems/{exerciseId}/test-cases.xml");

        return exerciseS3PartialTestCases.TestCases.Select(t => new
        {
            dbTestCase = exerciseDbPartialTestCases[t.TestCaseId],
            S3TestCase = t
        }).Select(t => new TestCaseJoined
        {
            Call = t.S3TestCase.Call,
            CallFunc = t.dbTestCase.CallFunc,
            Display = t.dbTestCase.Display,
            DisplayRes = t.dbTestCase.DisplayRes,
            Expected = t.S3TestCase.Expected,
            IsPublic = t.dbTestCase.IsPublic,
            ProblemProblemId = exerciseId,
            Setup = t.S3TestCase.Setup,
            TestCaseId = t.dbTestCase.TestCaseId
        }).ToList();
    }

    public async Task<AssistanceMessage> CreateNewChatMessage(ChatMessageInsertDto chatMessage,
        CancellationToken cancellationToken = default)
    {
        var chat = await dbContext.AssistantChats
            .Where(m => m.ProblemId == chatMessage.ProblemId && m.UserId == chatMessage.UserId &&
                        m.Name == chatMessage.ChatName)
            .FirstOrDefaultAsync(cancellationToken);

        if (chat == null)
        {
            chat = new AssistantChat
            {
                Name = chatMessage.ChatName,
                ProblemId = chatMessage.ProblemId,
                UserId = chatMessage.UserId,
            };
            dbContext.AssistantChats.Add(chat);
        }

        var fragments = chatMessage.TextFragments.Select(f => new AssistantMessageFragment
        {
            Content = f.Message,
            FragmentType = FragmentType.Text,
        }).ToList();

        fragments.AddRange(chatMessage.CodeFragments.Select(f => new AssistantMessageFragment
        {
            Content = f.Message,
            FragmentType = FragmentType.Code,
        }).ToList());

        var newMessage = new AssistanceMessage
        {
            ProblemId = chatMessage.ProblemId,
            UserId = chatMessage.UserId,
            ChatName = chatMessage.ChatName,
            Fragments = fragments,
            IsUserMessage = chatMessage.Author == MessageAuthor.User,
        };
        chat.Messages.Add(newMessage);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newMessage;
        throw new NotImplementedException();
    }
}