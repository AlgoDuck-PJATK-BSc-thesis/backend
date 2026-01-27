using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Shared.Utilities;

public sealed class DemoDataSeeder
{
    private readonly ApplicationCommandDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private sealed record AchievementDefinition(string Code, string Name, string Description, string Metric, int TargetValue);

    private static readonly IReadOnlyList<AchievementDefinition> AchievementDefinitions = new List<AchievementDefinition>
    {
        new("SOLVE_001", "First Steps", "Solve your first problem.", "amount_solved", 1),
        new("SOLVE_005", "Warmed Up", "Solve 5 problems.", "amount_solved", 5),
        new("SOLVE_010", "On a Roll", "Solve 10 problems.", "amount_solved", 10),
        new("SOLVE_025", "Quarter Century", "Solve 25 problems.", "amount_solved", 25),
        new("SOLVE_050", "Problem Grinder", "Solve 50 problems.", "amount_solved", 50),
        new("EXP_0100", "Getting Started", "Reach 100 XP.", "experience", 100),
        new("EXP_0500", "Leveling Up", "Reach 500 XP.", "experience", 500),
        new("EXP_1000", "Seasoned", "Reach 1,000 XP.", "experience", 1000),
        new("EXP_2500", "Battle Tested", "Reach 2,500 XP.", "experience", 2500),
        new("EXP_5000", "Veteran", "Reach 5,000 XP.", "experience", 5000),
        new("COIN_0100", "Pocket Change", "Hold 100 coins at once.", "coins", 100),
        new("COIN_1000", "Coin Collector", "Hold 1,000 coins at once.", "coins", 1000),
        new("COIN_5000", "Piggy Bank", "Hold 5,000 coins at once.", "coins", 5000),
        new("COIN_10000", "Treasure Chest", "Hold 10,000 coins at once.", "coins", 10000),
        new("COIN_25000", "Golden Hoard", "Hold 25,000 coins at once.", "coins", 25000)
    };

    private static readonly Guid AdminUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45");
    private static readonly Guid DefaultDuckItemId = Guid.Parse("016a1fce-3d78-46cd-8b25-b0f911c55644");

    private static readonly IReadOnlyList<(Guid Id, string Name, string JoinCode)> DemoCohorts = new List<(Guid, string, string)>
    {
        (Guid.Parse("0bb0d9f2-9b32-48f3-a194-b4a6ff1f3c11"), "Algorithms 101", "ALG101"),
        (Guid.Parse("a99e0c6f-6b3a-4c1b-a69b-03bd4aa6b264"), "Data Structures", "DS201"),
        (Guid.Parse("e7d6a56b-8f09-4bf6-8f0f-73a6a7ed3b17"), "Dynamic Programming", "DP301")
    };

    private static readonly IReadOnlyList<(Guid Id, string Username, string Email, Guid CohortId)> DemoMentors = new List<(Guid, string, string, Guid)>
    {
        (Guid.Parse("7f0d5f2b-4f84-4c2a-9c09-6c1c3a6a1a10"), "mentor_algo", "mentor_algo@algoduck.demo", Guid.Parse("0bb0d9f2-9b32-48f3-a194-b4a6ff1f3c11")),
        (Guid.Parse("3f5b2f76-66a4-4e6c-b0a8-3dbf4ef59a0a"), "mentor_ds", "mentor_ds@algoduck.demo", Guid.Parse("a99e0c6f-6b3a-4c1b-a69b-03bd4aa6b264")),
        (Guid.Parse("a7c1e8a2-3c18-49cb-9a4c-4f4a3b36f1b9"), "mentor_dp", "mentor_dp@algoduck.demo", Guid.Parse("e7d6a56b-8f09-4bf6-8f0f-73a6a7ed3b17"))
    };

    private static readonly DateTime AnchorUtc = new DateTime(2026, 1, 24, 0, 0, 0, DateTimeKind.Utc);

    public DemoDataSeeder(ApplicationCommandDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        var cohorts = await EnsureCohortsAsync();
        var users = await EnsureUsersAsync(cohorts);
        await EnsureUserDuckOwnershipsAsync(users);
        await EnsureUserSubmissionStatsAsync();
        await EnsureCohortMessagesAsync(cohorts, users);
        await EnsureUserAchievementsAsync();
    }

    private async Task<List<Cohort>> EnsureCohortsAsync()
    {
        var cohortIds = DemoCohorts.Select(x => x.Id).ToList();

        var existing = await _context.Cohorts.AsNoTracking()
            .Where(c => cohortIds.Contains(c.CohortId))
            .ToListAsync();

        var existingIds = existing.Select(c => c.CohortId).ToHashSet();
        var adminExists = await _context.ApplicationUsers.AsNoTracking().AnyAsync(u => u.Id == AdminUserId);

        foreach (var spec in DemoCohorts)
        {
            if (existingIds.Contains(spec.Id))
            {
                continue;
            }

            _context.Cohorts.Add(new Cohort
            {
                CohortId = spec.Id,
                Name = spec.Name,
                JoinCode = spec.JoinCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminExists ? AdminUserId : null,
                CreatedByUserLabel = adminExists ? "admin" : null,
                EmptiedAt = null
            });
        }

        await _context.SaveChangesAsync();

        return await _context.Cohorts.AsNoTracking()
            .Where(c => cohortIds.Contains(c.CohortId))
            .ToListAsync();
    }

    private async Task<List<ApplicationUser>> EnsureUsersAsync(IReadOnlyList<Cohort> cohorts)
    {
        var password = Environment.GetEnvironmentVariable("SEED_USER_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Missing env var: SEED_USER_PASSWORD");
        }

        var cohortIds = cohorts.Select(c => c.CohortId).ToArray();
        var createdOrUpdated = new List<ApplicationUser>();

        foreach (var mentor in DemoMentors)
        {
            var u = await EnsureUserAsync(
                mentor.Username,
                mentor.Email,
                password,
                mentor.Id,
                coins: 12000,
                experience: 3500,
                amountSolved: 25,
                cohortId: mentor.CohortId,
                cohortJoinedAt: AnchorUtc.AddDays(-21));

            createdOrUpdated.Add(u);
        }

        var desiredStudents = 60;

        var mentorNormalized = DemoMentors
            .Select(m => m.Username.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);

        var existingDemoStudents = await _context.ApplicationUsers.AsNoTracking()
            .Where(u =>
                u.Email != null &&
                u.Email.EndsWith("@algoduck.demo") &&
                u.NormalizedUserName != null &&
                u.NormalizedUserName != "ADMIN" &&
                !mentorNormalized.Contains(u.NormalizedUserName))
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var selectedExisting = existingDemoStudents.Take(desiredStudents).ToList();

        var usedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in await _context.ApplicationUsers.AsNoTracking().Select(u => u.UserName).ToListAsync())
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                usedUsernames.Add(name);
            }
        }

        foreach (var m in DemoMentors)
        {
            usedUsernames.Add(m.Username);
        }

        var studentsToEnsure = new List<(string Username, string Email)>();

        foreach (var u in selectedExisting)
        {
            if (string.IsNullOrWhiteSpace(u.UserName) || string.IsNullOrWhiteSpace(u.Email))
            {
                continue;
            }

            studentsToEnsure.Add((u.UserName, u.Email));
        }

        if (studentsToEnsure.Count < desiredStudents)
        {
            var missing = desiredStudents - studentsToEnsure.Count;

            for (var i = 0; i < missing; i++)
            {
                var username = BuildDeterministicStudentUsername(studentsToEnsure.Count + i + 1);
                username = EnsureNotUsed(username, usedUsernames);
                usedUsernames.Add(username);

                var email = $"{username}@algoduck.demo";
                studentsToEnsure.Add((username, email));
            }
        }

        for (var i = 0; i < studentsToEnsure.Count; i++)
        {
            var spec = studentsToEnsure[i];
            var stats = BuildStudentStats(i, cohortIds, spec.Username);

            var user = await EnsureUserAsync(
                spec.Username,
                spec.Email,
                password,
                null,
                stats.Coins,
                stats.Experience,
                stats.AmountSolved,
                stats.CohortId,
                stats.CohortJoinedAt);

            createdOrUpdated.Add(user);
        }

        return createdOrUpdated;
    }

    private static string EnsureNotUsed(string baseUsername, HashSet<string> used)
    {
        if (!used.Contains(baseUsername))
        {
            return baseUsername;
        }

        for (var i = 1; i <= 500; i++)
        {
            var candidate = $"{baseUsername}_{i}";
            if (!used.Contains(candidate))
            {
                return candidate;
            }
        }

        return Guid.NewGuid().ToString("N");
    }

    private static string BuildDeterministicStudentUsername(int index1Based)
    {
        var adjectives = new[]
        {
            "curious","brave","swift","quiet","sharp","calm","witty","bold","focused","eager",
            "steady","bright","clever","patient","nimble","methodic","careful","lucid","neat","solid"
        };

        var animals = new[]
        {
            "otter","fox","owl","wolf","lynx","badger","raven","panda","tiger","eagle",
            "koala","seal","yak","bison","gecko","shark","falcon","mole","whale","orca"
        };

        var i = Math.Max(1, index1Based);
        var adj = adjectives[(i - 1) % adjectives.Length];
        var animal = animals[((i - 1) * 3) % animals.Length];
        var num = 10 + (((i - 1) * 7) % 90);
        return $"{adj}_{animal}{num}";
    }

    private sealed record StudentStats(int Coins, int Experience, int AmountSolved, Guid CohortId, DateTime CohortJoinedAt);

    private static StudentStats BuildStudentStats(int index0Based, Guid[] cohortIds, string username)
    {
        var i = Math.Max(0, index0Based);
        var h = StableHash(username);
        var rng = new Random(HashCode.Combine(20260124, h, i));

        var cohortId = cohortIds.Length == 0 ? Guid.Empty : cohortIds[rng.Next(cohortIds.Length)];

        var coins = rng.Next(0, 28001);
        var experience = rng.Next(0, 6501);
        var amountSolved = rng.Next(0, 61);

        if (i == 0)
        {
            coins = 26000;
            experience = 5200;
            amountSolved = 55;
        }
        else if (i == 1)
        {
            coins = 10500;
            experience = 2600;
            amountSolved = 28;
        }
        else if (i == 2)
        {
            coins = 1200;
            experience = 650;
            amountSolved = 9;
        }

        var daysBack = rng.Next(7, 60);
        var joinedAt = AnchorUtc.AddDays(-daysBack);

        return new StudentStats(coins, experience, amountSolved, cohortId, joinedAt);
    }

    private static int StableHash(string s)
    {
        unchecked
        {
            uint hash = 2166136261;
            for (var i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= 16777619;
            }
            return (int)hash;
        }
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string username,
        string email,
        string password,
        Guid? userId,
        int coins,
        int experience,
        int amountSolved,
        Guid cohortId,
        DateTime cohortJoinedAt)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = userId ?? Guid.NewGuid(),
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                Coins = coins,
                Experience = experience,
                AmountSolved = amountSolved,
                CohortId = cohortId == Guid.Empty ? null : cohortId,
                CohortJoinedAt = cohortId == Guid.Empty ? null : cohortJoinedAt
            }.EnrichWithDefaults();

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var msg = string.Join(" | ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to create demo user '{username}': {msg}");
            }
        }
        else
        {
            var changed = false;

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    var msg = string.Join(" | ", setEmailResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Failed to update email for demo user '{username}': {msg}");
                }
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            if (user.Coins != coins) { user.Coins = coins; changed = true; }
            if (user.Experience != experience) { user.Experience = experience; changed = true; }
            if (user.AmountSolved != amountSolved) { user.AmountSolved = amountSolved; changed = true; }

            var targetCohortId = cohortId == Guid.Empty ? (Guid?)null : cohortId;
            var targetJoinedAt = cohortId == Guid.Empty ? (DateTime?)null : cohortJoinedAt;

            if (user.CohortId != targetCohortId)
            {
                user.CohortId = targetCohortId;
                user.CohortJoinedAt = targetJoinedAt;
                changed = true;
            }

            if (changed)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var msg = string.Join(" | ", updateResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Failed to update demo user '{username}': {msg}");
                }
            }
        }

        if (!await _userManager.IsInRoleAsync(user, "user"))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "user");
            if (!roleResult.Succeeded)
            {
                var msg = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to assign role to demo user '{username}': {msg}");
            }
        }

        return user;
    }

    private async Task EnsureUserDuckOwnershipsAsync(IReadOnlyList<ApplicationUser> users)
    {
        var userIds = users
            .Where(u => u.NormalizedUserName != "ADMIN")
            .Select(u => u.Id)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var duckItemIds = await _context.DuckItems
            .AsNoTracking()
            .Select(i => i.ItemId)
            .ToListAsync();

        if (duckItemIds.Count == 0)
        {
            return;
        }

        var eligibleAvatarItemIds = duckItemIds.Where(id => id != DefaultDuckItemId).ToList();
        if (eligibleAvatarItemIds.Count == 0)
        {
            return;
        }

        var existing = await _context.DuckOwnerships
            .Where(o => userIds.Contains(o.UserId))
            .ToListAsync();

        var byUser = existing
            .GroupBy(o => o.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var now = DateTime.UtcNow;

        foreach (var userId in userIds.OrderBy(x => x))
        {
            if (!byUser.TryGetValue(userId, out var owned))
            {
                owned = new List<DuckOwnership>();
                byUser[userId] = owned;
            }

            var rng = new Random(HashCode.Combine(20260124, userId.GetHashCode()));
            var ownedItemIds = owned.Select(o => o.ItemId).ToHashSet();

            var desiredOwned = Math.Min(duckItemIds.Count, rng.Next(2, 6));
            if (desiredOwned < 1)
            {
                desiredOwned = 1;
            }

            while (ownedItemIds.Count < desiredOwned)
            {
                var itemId = duckItemIds[rng.Next(duckItemIds.Count)];
                if (!ownedItemIds.Add(itemId))
                {
                    continue;
                }

                var purchase = new DuckOwnership
                {
                    UserId = userId,
                    ItemId = itemId,
                    PurchasedAt = now.AddDays(-rng.Next(1, 180)),
                    SelectedAsAvatar = false,
                    SelectedForPond = false
                };

                _context.DuckOwnerships.Add(purchase);
                owned.Add(purchase);
            }

            var hasEligibleOwned = owned.Any(o => o.ItemId != DefaultDuckItemId);
            if (!hasEligibleOwned)
            {
                var itemId = eligibleAvatarItemIds[rng.Next(eligibleAvatarItemIds.Count)];
                if (!ownedItemIds.Contains(itemId))
                {
                    var purchase = new DuckOwnership
                    {
                        UserId = userId,
                        ItemId = itemId,
                        PurchasedAt = now.AddDays(-rng.Next(1, 180)),
                        SelectedAsAvatar = false,
                        SelectedForPond = false
                    };

                    _context.DuckOwnerships.Add(purchase);
                    owned.Add(purchase);
                    ownedItemIds.Add(itemId);
                }
            }

            var eligibleForAvatar = owned.Where(o => o.ItemId != DefaultDuckItemId).ToList();
            if (eligibleForAvatar.Count == 0)
            {
                continue;
            }

            var selected = owned.Where(o => o.SelectedAsAvatar).ToList();
            var mustReselect =
                selected.Count != 1 ||
                selected[0].ItemId == DefaultDuckItemId ||
                !eligibleForAvatar.Any(x => x.ItemId == selected[0].ItemId);

            if (mustReselect)
            {
                foreach (var o in owned)
                {
                    o.SelectedAsAvatar = false;
                }

                eligibleForAvatar[rng.Next(eligibleForAvatar.Count)].SelectedAsAvatar = true;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureUserSubmissionStatsAsync()
    {
        var allUsers = await _context.ApplicationUsers
            .AsNoTracking()
            .Where(u => u.NormalizedUserName != "ADMIN")
            .ToListAsync();

        var userIds = allUsers
            .Select(u => u.Id)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var problemIds = await _context.Problems
            .AsNoTracking()
            .Select(p => p.ProblemId)
            .Take(50)
            .ToListAsync();

        if (problemIds.Count == 0)
        {
            return;
        }

        var existingStatsByUser = await _context.CodeExecutionStatisticss
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var now = DateTime.UtcNow;

        foreach (var user in allUsers)
        {
            if (existingStatsByUser.ContainsKey(user.Id) && existingStatsByUser[user.Id] > 0)
            {
                continue;
            }

            var submissionCount = Math.Min(user.AmountSolved * 3, 150);
            var acceptedCount = user.AmountSolved;

            if (submissionCount == 0)
            {
                continue;
            }

            var rng = new Random(HashCode.Combine(user.Id.GetHashCode(), 20260124));
            var solvedProblems = new HashSet<Guid>();

            for (var i = 0; i < submissionCount; i++)
            {
                var problemId = problemIds[rng.Next(problemIds.Count)];
                var isAccepted = solvedProblems.Count < acceptedCount && !solvedProblems.Contains(problemId);

                if (isAccepted)
                {
                    solvedProblems.Add(problemId);
                }

                TestCaseResult testResult;
                ExecutionResult execResult;

                if (isAccepted)
                {
                    testResult = TestCaseResult.Accepted;
                    execResult = ExecutionResult.Completed;
                }
                else
                {
                    var roll = rng.Next(100);
                    if (roll < 60)
                    {
                        testResult = TestCaseResult.Rejected;
                        execResult = ExecutionResult.Completed;
                    }
                    else if (roll < 80)
                    {
                        testResult = TestCaseResult.NotApplicable;
                        execResult = ExecutionResult.Timeout;
                    }
                    else
                    {
                        testResult = TestCaseResult.NotApplicable;
                        execResult = ExecutionResult.RuntimeError;
                    }
                }

                var daysBack = rng.Next(1, 30);
                var timestamp = now.AddDays(-daysBack);
                var timestampNanos = timestamp.DateTimeToNanos();

                _context.CodeExecutionStatisticss.Add(new CodeExecutionStatistics
                {
                    CodeExecutionId = Guid.NewGuid(),
                    UserId = user.Id,
                    ProblemId = problemId,
                    TestCaseResult = testResult,
                    Result = execResult,
                    ExecutionType = (JobType)1,
                    ExecutionStartNs = timestampNanos,
                    ExecutionEndNs = timestampNanos + (rng.Next(100, 5000) * 1_000_000L),
                    JvmPeakMemKb = rng.Next(10000, 50000),
                    ExitCode = execResult == ExecutionResult.Completed ? 0 : rng.Next(1, 255)
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureCohortMessagesAsync(IReadOnlyList<Cohort> cohorts, IReadOnlyList<ApplicationUser> users)
    {
        var desiredMinimum = 16;

        foreach (var cohort in cohorts)
        {
            var existingMessages = await _context.Set<Message>()
                .AsNoTracking()
                .Where(m => m.CohortId == cohort.CohortId)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.MessageId)
                .Select(m => new { m.CreatedAt, m.Message1 })
                .ToListAsync();

            var total = existingMessages.Count;
            var codeCount = existingMessages.Count(m => m.Message1.Contains("```java"));
            var penultimateIsCode = total >= 2 && existingMessages[total - 2].Message1.Contains("```java");

            if (total >= desiredMinimum && codeCount >= 3 && penultimateIsCode)
            {
                continue;
            }

            var memberIds = users.Where(u => u.CohortId == cohort.CohortId).Select(u => u.Id).Distinct().ToList();
            if (memberIds.Count == 0)
            {
                continue;
            }

            var mentorId = DemoMentors.First(m => m.CohortId == cohort.CohortId).Id;

            if (total == 0)
            {
                var baseTime = DateTime.UtcNow.AddDays(-21);
                var msgs = BuildMessages(cohort.CohortId, mentorId, memberIds, baseTime);
                _context.Set<Message>().AddRange(msgs);
                continue;
            }

            var lastCreatedAt = existingMessages[total - 1].CreatedAt;
            var baseTimeForAppend = lastCreatedAt.AddHours(2);
            var needed = Math.Max(0, desiredMinimum - total);

            var appendMsgs = BuildAppendMessages(cohort.CohortId, mentorId, memberIds, baseTimeForAppend, needed);
            _context.Set<Message>().AddRange(appendMsgs);
        }

        await _context.SaveChangesAsync();
    }

    private static List<Message> BuildMessages(Guid cohortId, Guid mentorId, List<Guid> memberIds, DateTime baseTime)
    {
        var students = memberIds.Where(id => id != mentorId).ToList();
        if (students.Count == 0)
        {
            students = memberIds;
        }

        Guid PickStudent(int i) => students[i % students.Count];

        string Clip(string s) => s.Length <= 256 ? s : s.Substring(0, 256);

        List<(Guid userId, string text)> script;

        if (cohortId == Guid.Parse("0bb0d9f2-9b32-48f3-a194-b4a6ff1f3c11"))
        {
            script = new List<(Guid, string)>
            {
                (mentorId, "Welcome to Algorithms 101. Big O is about growth with input size, not constant factors."),
                (PickStudent(0), "If constants do not matter, why do people micro-optimize?"),
                (mentorId, "Because Big O does not capture constant-time costs. For small inputs, constants and cache behavior matter."),
                (PickStudent(1), "Why is O(n log n) usually preferred over O(n squared)?"),
                (mentorId, "Because n log n grows much slower. For large inputs the gap dominates and performance becomes predictable."),
                (PickStudent(2), "Binary search:\n```java\nint lo=0,hi=n-1;\nwhile(lo<=hi){\n int m=(lo+hi)/2;\n if(a[m]==x) return m;\n if(a[m]<x) lo=m+1; else hi=m-1;\n}\nreturn -1;\n```\nIs this O(log n)?"),
                (mentorId, "Yes. Each loop halves the remaining search space, so time is O(log n) and space is O(1)."),
                (PickStudent(3), "What is a good way to analyze recursion complexity?"),
                (mentorId, "Write the recurrence, then match it to known patterns. Also track how many times each element is processed."),
                (PickStudent(4), "Counting operations:\n```java\nint ops=0;\nfor(int i=0;i<n;i++){\n for(int j=0;j<n;j++) ops++;\n}\n```\nQuadratic?"),
                (mentorId, "Correct. Two independent loops over n gives n*n operations, so O(n squared)."),
                (PickStudent(5), "Any common edge cases we should always test?"),
                (mentorId, "Empty input, one element, maximum constraints, duplicates, and negative numbers. Also watch integer overflow."),
                (PickStudent(6), "Is BFS always the best for shortest path?"),
                (mentorId, "Only on unweighted graphs. For weighted graphs use Dijkstra or another appropriate algorithm."),
                (PickStudent(7), "Sanity check:\n```java\nfor(int v:arr) sum+=v;\n```\nLinear time?"),
                (mentorId, "Yes. One pass over arr is linear time, and it uses constant extra space.")
            };
        }
        else if (cohortId == Guid.Parse("a99e0c6f-6b3a-4c1b-a69b-03bd4aa6b264"))
        {
            script = new List<(Guid, string)>
            {
                (mentorId, "Data Structures day: pick a structure that matches operations and complexity goals."),
                (PickStudent(0), "When is a linked list better than an array?"),
                (mentorId, "When you need frequent insertions or deletions and you already have node references. Arrays pay shifting costs."),
                (PickStudent(1), "Is this Node fine?\n```java\nstatic class Node{int v;Node next;}\n```\nI use it for most problems."),
                (mentorId, "Yes. It is a clean baseline for singly linked list tasks. Add prev only if you need backwards traversal."),
                (PickStudent(2), "Stack vs queue, how do you decide?"),
                (mentorId, "Stacks are LIFO and fit backtracking and DFS. Queues are FIFO and fit BFS and scheduling."),
                (PickStudent(3), "Hash map vs balanced tree, what is the tradeoff?"),
                (mentorId, "Hash maps give fast average lookup without ordering. Trees keep keys ordered and give predictable log worst case."),
                (PickStudent(4), "Hash map:\n```java\nMap<String,Integer> m=new HashMap<>();\nm.put(\"a\",1);\nint v=m.getOrDefault(\"a\",0);\n```\nAverage O(1)?"),
                (mentorId, "Yes, average O(1). Worst case can degrade, but in practice it is typically fast with good hashing."),
                (PickStudent(5), "What is a heap good for?"),
                (mentorId, "Priority queues: selecting k best items, scheduling, Dijkstra, and streaming problems."),
                (PickStudent(6), "Min-heap:\n```java\nPriorityQueue<Integer> pq=new PriorityQueue<>();\npq.add(3);pq.add(1);\nint x=pq.poll();\n```\npoll is O(log n)?"),
                (mentorId, "Correct. Heap insert and poll are O(log n), and peek is O(1).")
            };
        }
        else
        {
            script = new List<(Guid, string)>
            {
                (mentorId, "Dynamic Programming: define state, transition, and base cases. Then cache results."),
                (PickStudent(0), "How do I decide the DP state?"),
                (mentorId, "State should include exactly what affects future decisions. Too little breaks correctness, too much slows it down."),
                (PickStudent(1), "Memoization:\n```java\nint f(int i){\n if(i<=1) return i;\n if(m[i]!=-1) return m[i];\n return m[i]=f(i-1)+f(i-2);\n}\n```\nTop-down DP?"),
                (mentorId, "Yes. Base cases, cache check, compute and store is the standard flow."),
                (PickStudent(2), "Memoization or tabulation?"),
                (mentorId, "Memoization is usually faster to write. Tabulation is iterative and can reduce memory with rolling arrays."),
                (PickStudent(3), "Bottom-up:\n```java\ndp[0]=0;dp[1]=1;\nfor(int i=2;i<=n;i++) dp[i]=dp[i-1]+dp[i-2];\n```\nO(n) time?"),
                (mentorId, "Correct. You can reduce space to O(1) by keeping only the last two values."),
                (PickStudent(4), "Any standard DP problems to practice?"),
                (mentorId, "Knapsack, LIS, edit distance, and grid path counting are strong practice sets."),
                (PickStudent(5), "Rolling:\n```java\nint a=0,b=1;\nfor(int i=2;i<=n;i++){\n int c=a+b;\n a=b;b=c;\n}\n```\nO(1) space?"),
                (mentorId, "Yes. That is the classic space optimization when only recent states are needed.")
            };
        }

        var list = new List<Message>();
        for (var i = 0; i < script.Count; i++)
        {
            list.Add(new Message
            {
                MessageId = Guid.NewGuid(),
                CohortId = cohortId,
                UserId = script[i].userId,
                Message1 = Clip(script[i].text),
                CreatedAt = baseTime.AddHours(i * 3),
                MediaType = 0,
                MediaKey = null,
                MediaContentType = null
            });
        }

        return list;
    }

    private static List<Message> BuildAppendMessages(Guid cohortId, Guid mentorId, List<Guid> memberIds, DateTime baseTime, int requiredAdditionalCount)
    {
        var students = memberIds.Where(id => id != mentorId).ToList();
        if (students.Count == 0)
        {
            students = memberIds;
        }

        Guid PickStudent(int i) => students[i % students.Count];

        string Clip(string s) => s.Length <= 256 ? s : s.Substring(0, 256);

        List<(Guid userId, string text)> filler;
        List<(Guid userId, string text)> core;

        if (cohortId == Guid.Parse("0bb0d9f2-9b32-48f3-a194-b4a6ff1f3c11"))
        {
            filler = new List<(Guid, string)>
            {
                (PickStudent(0), "How do you decide between sorting first vs using a hash-based approach?"),
                (mentorId, "Sorting can simplify logic and enable two-pointers. Hashing can give linear time but uses extra memory."),
                (PickStudent(1), "Is amortized analysis relevant in interviews?"),
                (mentorId, "Yes. It explains dynamic arrays where most operations are O(1) but occasional resizes cost more."),
                (PickStudent(2), "What is a quick way to estimate complexity from loops?"),
                (mentorId, "Count how many times the inner body can run as a function of n, then keep the dominant term.")
            };

            core = new List<(Guid, string)>
            {
                (PickStudent(3), "Binary search:\n```java\nint lo=0,hi=n-1;\nwhile(lo<=hi){\n int m=(lo+hi)/2;\n if(a[m]==x) return m;\n if(a[m]<x) lo=m+1; else hi=m-1;\n}\nreturn -1;\n```"),
                (mentorId, "That pattern is correct for sorted arrays. Keep an eye on mid overflow in other languages."),
                (PickStudent(4), "Two loops:\n```java\nfor(int i=0;i<n;i++){\n for(int j=0;j<n;j++) work();\n}\n```\nQuadratic?"),
                (mentorId, "Yes. If inner bounds depend on i, re-check because it can become triangular."),
                (PickStudent(5), "One pass:\n```java\nfor(int v:arr) sum+=v;\n```\nLinear time?"),
                (mentorId, "Correct. One pass is linear, and the accumulator is constant space.")
            };
        }
        else if (cohortId == Guid.Parse("a99e0c6f-6b3a-4c1b-a69b-03bd4aa6b264"))
        {
            filler = new List<(Guid, string)>
            {
                (PickStudent(0), "When should I prefer ArrayDeque over Stack?"),
                (mentorId, "In Java, ArrayDeque is generally preferred for stack and queue usage."),
                (PickStudent(1), "How do you approach tree traversal questions quickly?"),
                (mentorId, "Pick BFS for levels, otherwise DFS for structure and recursion patterns."),
                (PickStudent(2), "What is a simple way to detect cycles in a linked list?"),
                (mentorId, "Use slow and fast pointers. If they meet, there is a cycle.")
            };

            core = new List<(Guid, string)>
            {
                (PickStudent(3), "Node:\n```java\nstatic class Node{int v;Node next;}\n```"),
                (mentorId, "That is sufficient for most problems."),
                (PickStudent(4), "Hash map:\n```java\nMap<Integer,Integer> m=new HashMap<>();\nm.put(x,i);\nInteger j=m.get(y);\n```"),
                (mentorId, "Average lookup is O(1). Handle missing keys safely."),
                (PickStudent(5), "Heap:\n```java\nPriorityQueue<Integer> pq=new PriorityQueue<>();\npq.add(3);pq.add(1);\nint x=pq.poll();\n```"),
                (mentorId, "poll is O(log n). peek is O(1).")
            };
        }
        else
        {
            filler = new List<(Guid, string)>
            {
                (PickStudent(0), "How do you know if greedy will fail and DP is needed?"),
                (mentorId, "If local choices impact future options and counterexamples exist, DP is usually safer."),
                (PickStudent(1), "What is the first thing you write when starting a DP solution?"),
                (mentorId, "Define the state in one sentence, then write transition and base cases."),
                (PickStudent(2), "Any advice to avoid off-by-one errors in DP arrays?"),
                (mentorId, "Be explicit about dp[i] meaning and validate on tiny inputs.")
            };

            core = new List<(Guid, string)>
            {
                (PickStudent(3), "Caching:\n```java\nif(m[i]!=-1) return m[i];\n```"),
                (mentorId, "Yes. It prevents recomputation."),
                (PickStudent(4), "Tabulation:\n```java\nfor(int i=1;i<=n;i++){\n dp[i]=best(dp[i-1],dp[i-2]);\n}\n```"),
                (mentorId, "Typical bottom-up shape. Ensure base cases are correct."),
                (PickStudent(5), "Rolling:\n```java\nint a=0,b=1;\nfor(int i=2;i<=n;i++){\n int c=a+b;\n a=b;b=c;\n}\n```"),
                (mentorId, "That keeps O(1) space.")
            };
        }

        var fillerCount = Math.Max(0, requiredAdditionalCount - core.Count);
        var chosenFiller = new List<(Guid userId, string text)>();

        if (fillerCount > 0 && filler.Count > 0)
        {
            for (var i = 0; i < fillerCount; i++)
            {
                chosenFiller.Add(filler[i % filler.Count]);
            }
        }

        var script = new List<(Guid userId, string text)>();
        script.AddRange(chosenFiller);
        script.AddRange(core);

        var list = new List<Message>();
        for (var i = 0; i < script.Count; i++)
        {
            list.Add(new Message
            {
                MessageId = Guid.NewGuid(),
                CohortId = cohortId,
                UserId = script[i].userId,
                Message1 = Clip(script[i].text),
                CreatedAt = baseTime.AddHours(i * 3),
                MediaType = 0,
                MediaKey = null,
                MediaContentType = null
            });
        }

        return list;
    }

    private async Task EnsureAchievementsAsync()
    {
        var existingCodes = await _context.Achievements
            .Select(a => a.Code)
            .ToListAsync();

        var existingSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var def in AchievementDefinitions)
        {
            if (existingSet.Contains(def.Code))
            {
                var existing = await _context.Achievements.FindAsync(def.Code);
                if (existing != null)
                {
                    existing.Name = def.Name;
                    existing.Description = def.Description;
                    existing.TargetValue = def.TargetValue;
                }
            }
            else
            {
                _context.Achievements.Add(new Achievement
                {
                    Code = def.Code,
                    Name = def.Name,
                    Description = def.Description,
                    TargetValue = def.TargetValue,
                    CreatedAt = now
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureUserAchievementsAsync()
    {
        await EnsureAchievementsAsync();

        var dbUsers = await _context.ApplicationUsers.AsNoTracking()
            .Where(u => u.NormalizedUserName != null && u.NormalizedUserName != "ADMIN")
            .Select(u => new
            {
                u.Id,
                u.AmountSolved,
                u.Experience,
                u.Coins
            })
            .ToListAsync();

        if (dbUsers.Count == 0)
        {
            return;
        }

        var userIds = dbUsers.Select(u => u.Id).Distinct().ToList();

        var existing = await _context.UserAchievements
            .Where(a => userIds.Contains(a.UserId))
            .ToListAsync();

        var map = existing.ToDictionary(a => (a.UserId, a.AchievementCode), a => a);

        var now = DateTime.UtcNow;

        foreach (var user in dbUsers)
        {
            foreach (var def in AchievementDefinitions)
            {
                var key = (user.Id, def.Code);
                var current = ComputeMetricValue(user.AmountSolved, user.Experience, user.Coins, def.Metric);
                var completed = current >= def.TargetValue;

                if (!map.TryGetValue(key, out var entity))
                {
                    entity = new UserAchievement
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        AchievementCode = def.Code,
                        CurrentValue = completed ? def.TargetValue : current,
                        IsCompleted = completed,
                        CreatedAt = now,
                        CompletedAt = completed ? now : null
                    };

                    _context.UserAchievements.Add(entity);
                    map[key] = entity;
                    continue;
                }

                entity.IsCompleted = completed;
                entity.CurrentValue = completed ? def.TargetValue : current;
                entity.CompletedAt = completed ? (entity.CompletedAt ?? now) : null;
            }
        }

        await _context.SaveChangesAsync();
    }

    private static int ComputeMetricValue(int amountSolved, int experience, int coins, string metric)
    {
        if (string.Equals(metric, "amount_solved", StringComparison.OrdinalIgnoreCase)) return amountSolved;
        if (string.Equals(metric, "experience", StringComparison.OrdinalIgnoreCase)) return experience;
        if (string.Equals(metric, "coins", StringComparison.OrdinalIgnoreCase)) return coins;
        return 0;
    }
}
