using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Shared.Utilities;

public class DataSeedingService
{
    private readonly ApplicationCommandDbContext _context;
    private readonly IAwsS3Client _s3Client;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDefaultDuckService _defaultDuckService;

    public DataSeedingService(IDefaultDuckService defaultDuckService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IAwsS3Client s3Client, ApplicationCommandDbContext context)
    {
        _defaultDuckService = defaultDuckService;
        _userManager = userManager;
        _roleManager = roleManager;
        _s3Client = s3Client;
        _context = context;
    }

    public async Task SeedDataAsync()
    {
        await SeedEditorLayouts();
        await SeedEditorThemes();
        await SeedRolesAsync();
        await SeedSeededUsersAsync();
        await SeedUserConfigsAsync();
        await SeedRarities();
        await SeedCategories();
        await SeedDifficulties();
        await SeedItems();
        await EnsureDefaultsForSeededUsersAsync();
        await SeedProblems();
        await SeedTestCases();
        await new DemoDataSeeder(_context, _userManager).SeedAsync();
        await SeedUserConfigsAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = ["admin", "user"];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }
    }

    private async Task SeedUserConfigsAsync()
    {
        var userIds = await _context.ApplicationUsers
            .AsNoTracking()
            .Select(u => u.Id)
            .ToListAsync();

        if (userIds.Count == 0)
        {
            return;
        }

        var existingConfigUserIds = await _context.UserConfigs
            .AsNoTracking()
            .Select(c => c.UserId)
            .ToListAsync();

        var missing = userIds.Except(existingConfigUserIds).ToList();

        if (missing.Count == 0)
        {
            return;
        }

        var configs = missing.Select(id => new UserConfig
        {
            UserId = id,
            EditorFontSize = 11,
            EmailNotificationsEnabled = false,
            IsDarkMode = true,
            IsHighContrast = false
        }).ToList();

        await _context.UserConfigs.AddRangeAsync(configs);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedSeededUsersAsync()
    {
        var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
        var userPassword = Environment.GetEnvironmentVariable("SEED_USER_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Missing env var: SEED_ADMIN_PASSWORD");
        }

        if (string.IsNullOrWhiteSpace(userPassword))
        {
            throw new InvalidOperationException("Missing env var: SEED_USER_PASSWORD");
        }

        await EnsureUserWithRoleAsync(
            username: "admin",
            email: "algoduckpl@gmail.com",
            password: adminPassword,
            role: "admin",
            userId: Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"));

        await EnsureUserWithRoleAsync(
            username: "algoduck",
            email: "algoduckpl+user@gmail.com",
            password: userPassword,
            role: "user",
            userId: Guid.Parse("b3c38fed-69c5-4063-9d54-7cb4199dfdab"));
    }

    private async Task EnsureDefaultsForSeededUsersAsync()
    {
        await _defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(
            Guid.Parse("b3c38fed-69c5-4063-9d54-7cb4199dfdab"),
            CancellationToken.None);
        
        await _defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(
            Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
            CancellationToken.None);
    }

    private async Task EnsureUserWithRoleAsync(string username, string email, string password, string role, Guid? userId = null)
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
                Coins = 50000,
                Experience = 1000,
                AmountSolved = 10
            }.EnrichWithDefaults();

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create seeded '{username}' user: {IdentityErrorsToString(createResult)}");
            }
        }
        else
        {
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to update email for '{username}': {IdentityErrorsToString(setEmailResult)}");
                }
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to confirm email for '{username}': {IdentityErrorsToString(updateResult)}");
                }
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign role '{role}' to '{username}': {IdentityErrorsToString(roleResult)}");
            }
        }
    }

    private static string IdentityErrorsToString(IdentityResult result)
    {
        var parts = new List<string>();
        foreach (var e in result.Errors)
        {
            parts.Add($"{e.Code}: {e.Description}");
        }

        return string.Join(" | ", parts);
    }


    private async Task SeedRarities()
    {
        if (!await _context.Rarities.AnyAsync())
        {
            var rarities = new List<Rarity>
            {
                new()
                {
                    RarityId = Guid.Parse("016a1fce-3d78-46cd-8b25-b0f911c55642"), RarityName = "COMMON",
                    RarityLevel = 1
                },
                new()
                {
                    RarityId = Guid.Parse("ea1da060-6add-423e-a5bc-cc81d31f98ac"), RarityName = "UNCOMMON",
                    RarityLevel = 2
                },
                new()
                {
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"), RarityName = "RARE", RarityLevel = 3
                },
                new()
                {
                    RarityId = Guid.Parse("c86c74ea-109a-4402-8606-c653d117edf2"), RarityName = "EPIC", RarityLevel = 4
                },
                new()
                {
                    RarityId = Guid.Parse("f3b9d57f-0c2f-444e-938f-57fd2782bf0a"), RarityName = "LEGENDARY",
                    RarityLevel = 5
                }
            };

            await _context.Rarities.AddRangeAsync(rarities);
            await _context.SaveChangesAsync();
        }
    }

    
    private async Task SeedEditorLayouts()
    {

        if (!await _context.EditorLayouts.AnyAsync())
        {
            var layouts = new List<EditorLayout>
            {
                new()
                {
                    EditorLayoutId = Guid.Parse("7d2e1c42-f7da-4261-a8c1-42826d976116"),
                    LayoutName = "default",
                    IsDefaultLayout = true
                },
                new()
                {
                    EditorLayoutId = Guid.Parse("3922523c-7c2f-4a9a-9f43-9fc5b8698972"),
                    LayoutName = "I use wayland btw",
                    IsDefaultLayout = true
                },
                new()
                {
                    EditorLayoutId = Guid.Parse("b9647438-6bec-45e6-a942-207dc40be273"),
                    LayoutName = "Tab enjoyer",
                    IsDefaultLayout = true
                }
            };

            await _context.EditorLayouts.AddRangeAsync(layouts);
            await _context.SaveChangesAsync();

            var layoutContentsRaw = new List<EditorLayoutS3Partial>
            {
                new()
                {
                    Id = Guid.Parse("7d2e1c42-f7da-4261-a8c1-42826d976116"),
                    ConfigObjectRaw =
                        "{\n  \"compId\": \"root-wrapper\",\n  \"component\": \"TopLevelComponent\",\n  \"options\": {\n    \"component\": {\n      \"compId\": \"root\",\n      \"component\": \"SplitPanel\",\n      \"options\": {\n        \"axis\": 0,\n        \"initialComp1Proportions\": 0.25,\n        \"comp1\": {\n          \"compId\": \"problem-info-wrapper\",\n          \"component\": \"TopLevelComponent\",\n          \"options\": {\n            \"component\": {\n              \"compId\": \"problem-info\",\n              \"component\": \"InfoPanel\",\n              \"options\": {},\n              \"meta\": {\n                \"clampVal\": 0.025,\n                \"clamp\": {\n                  \"component\": \"ProblemInfoClamp\",\n                  \"options\": {}\n                }\n              }\n            }\n          }\n        },\n        \"comp2\": {\n          \"compId\": \"coding-area-wrapper\",\n          \"component\": \"TopLevelComponent\",\n          \"options\": {\n            \"component\": {\n                \"meta\": {\n            \"clampVal\": 0.025,\n            \"clamp\": {\n              \"component\": \"CodingAreaClamp\",\n              \"options\": {}\n            }\n          },\n              \"compId\": \"coding-area\",\n              \"component\": \"SplitPanel\",\n              \"options\": {\n                \"axis\": 1,\n                \"initialComp1Proportions\": 0.75,\n                \"comp1\": {\n                  \"compId\": \"code-editor-wrapper\",\n                  \"component\": \"TopLevelComponent\",\n                  \"options\": {\n                    \"component\": {\n                      \"compId\": \"code-editor\",\n                      \"component\": \"Editor\",\n                      \"options\": {},\n                      \"meta\": {\n                        \"clampVal\": 0.025,\n                        \"clamp\": {\n                          \"component\": \"Editor\",\n                          \"options\": {}\n                        }\n                      }\n                    }\n                  }\n                },\n                \"comp2\": {\n                  \"compId\": \"solution-data-area-wrapper\",\n                  \"component\": \"TopLevelComponent\",\n                  \"options\": {\n                    \"component\": {\n                      \"compId\": \"solution-data-area\",\n                      \"component\": \"WizardPanel\",\n                      \"options\": {\n                        \"side\": 1,\n                        \"control\": {\n                          \"compId\": \"control-panel\",\n                          \"component\": \"SolutionAreaControlPanel\",\n                          \"options\": {}\n                        },\n                        \"groups\": [],\n                        \"components\": [\n                          {\n                            \"compId\": \"terminal-comp-wrapper\",\n                            \"component\": \"TopLevelComponent\",\n                            \"options\": {\n                              \"component\": {\n                                \"compId\": \"terminal-comp\",\n                                \"component\": \"Terminal\",\n                                \"options\": {},\n                                \"meta\": {\n                                  \"label\": {\n                                    \"commonName\": \"Terminal\",\n                                    \"labelFor\": \"terminal-comp\",\n                                    \"icon\": {\n                                      \"component\": \"TerminalIconSvg\",\n                                      \"options\": {}\n                                    }\n                                  }\n                                }\n                              }\n                            }\n                          },\n                          {\n                            \"compId\": \"test-cases-comp-wrapper\",\n                            \"component\": \"TopLevelComponent\",\n                            \"options\": {\n                              \"component\": {\n                                \"compId\": \"test-cases-comp\",\n                                \"component\": \"TestCases\",\n                                \"options\": {},\n                                \"meta\": {\n                                  \"label\": {\n                                    \"commonName\": \"Test Cases\",\n                                    \"labelFor\": \"test-cases-comp\",\n                                    \"icon\": {\n                                      \"component\": \"TestCasesIconSvg\",\n                                      \"options\": {}\n                                    }\n                                  }\n                                }\n                              }\n                            }\n                          },\n                          {\n  \"compId\": \"assistant-wizard-wrapper\",\n  \"component\": \"TopLevelComponent\",\n  \"options\": {\n    \"component\": {\n      \"compId\": \"assistant-wizard\",\n      \"component\": \"WizardPanel\",\n      \"options\": {\n        \"side\": 3,\n        \"control\": {\n          \"compId\": \"wizard-control-panel\",\n          \"component\": \"AssistantWizardControlPanel\",\n          \"options\": {}\n        },\n        \"groups\": [],\n        \"components\": []\n      },\n      \"meta\": {\n        \"clampVal\": 0.025,\n        \"clamp\": {\n          \"component\": \"AssistantWizardClamp\",\n          \"options\": {}\n        },\n        \"label\": {\n          \"commonName\": \"Assistant\",\n          \"labelFor\": \"assistant-wizard\",\n          \"icon\": {\n            \"component\": \"AssistantIconSvg\",\n            \"options\": {}\n          }\n        }\n      }\n    }\n  }\n}\n                        ],\n                  \"meta\": {\n                    \"clampVal\": 0.025,\n                    \"clamp\": {\n                      \"component\": \"SolutionDataAreaClamp\",\n                      \"options\": {}\n                    }\n                  }\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n          \n        }\n      }\n    }\n  }\n}"
                },
                new()
                {
                    Id = Guid.Parse("3922523c-7c2f-4a9a-9f43-9fc5b8698972"),
                    ConfigObjectRaw =
                        "{\n  \"compId\": \"root-1767541429676-0\",\n  \"component\": \"TopLevelComponent\",\n  \"options\": {\n    \"component\": {\n      \"compId\": \"placeholder-1767541429676-1\",\n      \"component\": \"SplitPanel\",\n      \"options\": {\n        \"axis\": 0,\n        \"comp1\": {\n          \"compId\": \"wrapper-1767541431591-3\",\n          \"component\": \"TopLevelComponent\",\n          \"options\": {\n            \"component\": {\n              \"compId\": \"assistant-wizard\",\n              \"component\": \"TerminalWizardPanel\",\n              \"options\": {\n                \"components\": [],\n                \"side\": 3,\n                \"control\": {\n                  \"compId\": \"wizard-control-panel\",\n                  \"component\": \"AssistantWizardControlPanel\",\n                  \"options\": {}\n                }\n              },\n              \"meta\": {\n                \"label\": {\n                  \"commonName\": \"Assistant\",\n                  \"labelFor\": \"assistant-wizard\",\n                  \"icon\": {\n                    \"compId\": \"assistant-icon\",\n                    \"component\": \"AssistantIconSvg\",\n                    \"options\": {}\n                  }\n                }\n              }\n            }\n          }\n        },\n        \"comp2\": {\n          \"compId\": \"wrapper-1767541431591-5\",\n          \"component\": \"TopLevelComponent\",\n          \"options\": {\n            \"component\": {\n              \"compId\": \"inner-1767541431591-6\",\n              \"component\": \"SplitPanel\",\n              \"options\": {\n                \"axis\": 1,\n                \"comp1\": {\n                  \"compId\": \"wrapper-1767541434961-9\",\n                  \"component\": \"TopLevelComponent\",\n                  \"options\": {\n                    \"component\": {\n                      \"compId\": \"inner-1767541434961-10\",\n                      \"component\": \"SplitPanel\",\n                      \"options\": {\n                        \"axis\": 0,\n                        \"comp1\": {\n                          \"compId\": \"wrapper-1767541438744-17\",\n                          \"component\": \"TopLevelComponent\",\n                          \"options\": {\n                            \"component\": {\n                              \"compId\": \"terminal-comp\",\n                              \"component\": \"Terminal\",\n                              \"options\": {},\n                              \"meta\": {\n                                \"label\": {\n                                  \"commonName\": \"Terminal\",\n                                  \"labelFor\": \"terminal-comp\",\n                                  \"icon\": {\n                                    \"compId\": \"someComp\",\n                                    \"component\": \"EditorIconSvg\",\n                                    \"options\": {}\n                                  }\n                                }\n                              }\n                            }\n                          }\n                        },\n                        \"comp2\": {\n                          \"compId\": \"wrapper-1767541438744-19\",\n                          \"component\": \"TopLevelComponent\",\n                          \"options\": {\n                            \"component\": {\n                              \"compId\": \"inner-1767541438744-20\",\n                              \"component\": \"SplitPanel\",\n                              \"options\": {\n                                \"axis\": 1,\n                                \"comp1\": {\n                                  \"compId\": \"wrapper-1767541441490-23\",\n                                  \"component\": \"TopLevelComponent\",\n                                  \"options\": {\n                                    \"component\": {\n                                      \"compId\": \"test-cases-comp\",\n                                      \"component\": \"TestCases\",\n                                      \"options\": {},\n                                      \"meta\": {\n                                        \"label\": {\n                                          \"commonName\": \"Test Cases\",\n                                          \"labelFor\": \"test-cases-comp\",\n                                          \"icon\": {\n                                            \"compId\": \"someComp\",\n                                            \"component\": \"TestCasesIconSvg\",\n                                            \"options\": {}\n                                          }\n                                        }\n                                      }\n                                    }\n                                  }\n                                },\n                                \"comp2\": {\n                                  \"compId\": \"wrapper-1767541441490-25\",\n                                  \"component\": \"TopLevelComponent\",\n                                  \"options\": {\n                                    \"component\": {\n                                      \"compId\": \"code-editor\",\n                                      \"component\": \"Editor\",\n                                      \"options\": {},\n                                      \"meta\": {\n                                        \"label\": {\n                                          \"commonName\": \"Code Editor\",\n                                          \"labelFor\": \"code-editor\",\n                                          \"icon\": {\n                                            \"compId\": \"someComp\",\n                                            \"component\": \"TerminalIconSvg\",\n                                            \"options\": {}\n                                          }\n                                        }\n                                      }\n                                    }\n                                  }\n                                },\n                                \"initialComp1Proportions\": 0.5\n                              }\n                            }\n                          }\n                        },\n                        \"initialComp1Proportions\": 0.2899061032863847\n                      }\n                    }\n                  }\n                },\n                \"comp2\": {\n                  \"compId\": \"wrapper-1767541434961-11\",\n                  \"component\": \"TopLevelComponent\",\n                  \"options\": {\n                    \"component\": {\n                      \"compId\": \"problem-info\",\n                      \"component\": \"InfoPanel\",\n                      \"options\": {},\n                      \"meta\": {\n                        \"label\": {\n                          \"commonName\": \"Problem Info\",\n                          \"labelFor\": \"problem-info\",\n                          \"icon\": {\n                            \"compId\": \"someComp\",\n                            \"component\": \"InfoPanelIconSvg\",\n                            \"options\": {}\n                          }\n                        }\n                      }\n                    }\n                  }\n                },\n                \"initialComp1Proportions\": 0.6623188405797109\n              }\n            }\n          }\n        },\n        \"initialComp1Proportions\": 0.33896620278330153\n      }\n    }\n  }\n}"
                },
                new()
                {
                    Id = Guid.Parse("b9647438-6bec-45e6-a942-207dc40be273"),
                    ConfigObjectRaw =
                        "{\n  \"compId\": \"root-1767198734054-0\",\n  \"component\": \"TopLevelComponent\",\n  \"options\": {\n    \"component\": {\n      \"compId\": \"wizard-1767198739318-7\",\n      \"component\": \"WizardPanel\",\n      \"options\": {\n        \"components\": [\n          {\n            \"compId\": \"wrapper-1767198741431-9\",\n            \"component\": \"TopLevelComponent\",\n            \"options\": {\n              \"component\": {\n                \"compId\": \"terminal-comp\",\n                \"component\": \"Terminal\",\n                \"options\": {},\n                \"meta\": {\n                  \"label\": {\n                    \"commonName\": \"Terminal\",\n                    \"labelFor\": \"terminal-comp\",\n                    \"icon\": {\n                      \"compId\": \"someComp\",\n                      \"component\": \"EditorIconSvg\",\n                      \"options\": {}\n                    }\n                  }\n                }\n              }\n            }\n          },\n          {\n            \"compId\": \"wrapper-1767198742451-12\",\n            \"component\": \"TopLevelComponent\",\n            \"options\": {\n              \"component\": {\n                \"compId\": \"problem-info\",\n                \"component\": \"InfoPanel\",\n                \"options\": {},\n                \"meta\": {\n                  \"label\": {\n                    \"commonName\": \"Problem Info\",\n                    \"labelFor\": \"problem-info\",\n                    \"icon\": {\n                      \"compId\": \"someComp\",\n                      \"component\": \"InfoPanelIconSvg\",\n                      \"options\": {}\n                    }\n                  }\n                }\n              }\n            }\n          },\n          {\n            \"compId\": \"wrapper-1767198743532-15\",\n            \"component\": \"TopLevelComponent\",\n            \"options\": {\n              \"component\": {\n                \"compId\": \"test-cases-comp\",\n                \"component\": \"TestCases\",\n                \"options\": {},\n                \"meta\": {\n                  \"label\": {\n                    \"commonName\": \"Test Cases\",\n                    \"labelFor\": \"test-cases-comp\",\n                    \"icon\": {\n                      \"compId\": \"someComp\",\n                      \"component\": \"TestCasesIconSvg\",\n                      \"options\": {}\n                    }\n                  }\n                }\n              }\n            }\n          },\n          {\n            \"compId\": \"wrapper-1767198744548-18\",\n            \"component\": \"TopLevelComponent\",\n            \"options\": {\n              \"component\": {\n                \"compId\": \"code-editor\",\n                \"component\": \"Editor\",\n                \"options\": {},\n                \"meta\": {\n                  \"label\": {\n                    \"commonName\": \"Code Editor\",\n                    \"labelFor\": \"code-editor\",\n                    \"icon\": {\n                      \"compId\": \"someComp\",\n                      \"component\": \"TerminalIconSvg\",\n                      \"options\": {}\n                    }\n                  }\n                }\n              }\n            }\n          },\n          {\n            \"compId\": \"wrapper-1767198745298-21\",\n            \"component\": \"TopLevelComponent\",\n            \"options\": {\n              \"component\": {\n                \"compId\": \"assistant-wizard\",\n                \"component\": \"TerminalWizardPanel\",\n                \"options\": {\n                  \"components\": [],\n                  \"side\": 3,\n                  \"control\": {\n                    \"compId\": \"wizard-control-panel\",\n                    \"component\": \"AssistantWizardControlPanel\",\n                    \"options\": {}\n                  }\n                },\n                \"meta\": {\n                  \"label\": {\n                    \"commonName\": \"Assistant\",\n                    \"labelFor\": \"assistant-wizard\",\n                    \"icon\": {\n                      \"compId\": \"assistant-icon\",\n                      \"component\": \"AssistantIconSvg\",\n                      \"options\": {}\n                    }\n                  }\n                }\n              }\n            }\n          }\n        ],\n        \"side\": 0,\n        \"control\": {\n          \"compId\": \"control-1767198739318-8\",\n          \"component\": \"SolutionAreaControlPanelHorizontal\",\n          \"options\": {}\n        }\n      }\n    }\n  }\n}"
                }
            };

            foreach (var info in layoutContentsRaw)
            {
                var objectPath =
                    $"users/layouts/{info.Id}.json";
                var objectExistsResult = await _s3Client.ObjectExistsAsync(objectPath);
                if (objectExistsResult is { IsOk: true, AsOk: false })
                {
                    await _s3Client.PostJsonObjectAsync(objectPath,
                        JsonSerializer.Deserialize<object>(info.ConfigObjectRaw)!);
                }
            }
        }
        
    }

    private async Task SeedCategories()
    {
        if (!await _context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                    CategoryName = "Data Structures"
                },
                new Category
                {
                    CategoryId = Guid.Parse("5c721265-24a9-4ed8-8214-f415d4a9bede"),
                    CategoryName = "Arrays & Hashing"
                },
                new Category
                {
                    CategoryId = Guid.Parse("3b676e51-aa3c-40d5-af15-1cfe04b52c37"),
                    CategoryName = "Strings"
                },
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedDifficulties()
    {
        if (!await _context.Difficulties.AnyAsync())
        {
            await _context.Difficulties.AddRangeAsync(new Difficulty
                {
                    DifficultyId = Guid.Parse("79f9390e-4b7f-4c1f-a615-b1c6e2caa411"),
                    DifficultyName = "EASY",
                    RewardScaler = 1m
                },
                new Difficulty
                {
                    DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
                    DifficultyName = "MEDIUM",
                    RewardScaler = 1.7m
                },
                new Difficulty
                {
                    DifficultyId = Guid.Parse("dc08e91d-c0cd-4dee-80d9-30d7634e0917"),
                    DifficultyName = "HARD",
                    RewardScaler = 2.5m
                }
            );
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedItems()
    {
        if (!await _context.Items.AnyAsync())
        {
            var items = new List<Item>
            {
                new DuckItem
                {
                    ItemId = Guid.Parse("16d4a949-0f5f-481a-b9d6-e0329f9d7dd3"),
                    Name = "pirate",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("052b219a-ec0b-430a-a7db-95c5db35dfce"),
                    Name = "detective",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("03a4dced-f802-4cc5-b239-e0d4c3be9dcd"),
                    Name = "princess",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("182769e6-ff23-4584-a6fd-83d1c71725e8"),
                    Name = "miku",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("3cf1b82e-704a-4f2b-8bc0-af22b41dec14"),
                    Name = "mermaid",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("56ef2a57-707e-43d4-b62e-0c69ed4e8c65"),
                    Name = "anakin",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("6239a5ed-45e7-4316-80c2-b3b4c7eeab5f"),
                    Name = "samurai",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("660d65f2-6b1f-49c0-ac05-cfc0af7dc080"),
                    Name = "ninja",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("6e231d75-91ff-4112-8d25-7f289b6e6f04"),
                    Name = "viking",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("833e927f-55cf-43e1-9653-647819e09bf2"),
                    Name = "knight",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("88ae6422-cb6f-4245-8367-cf46e381d886"),
                    Name = "cowboy",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("8e32fcf2-a192-4cd1-ad41-2e4362b6007d"),
                    Name = "witch",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new DuckItem
                {
                    ItemId = Guid.Parse("be99f3f8-412a-4503-99d6-52fee988ad88"),
                    Name = "mallard",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },

                new DuckItem
                {
                    ItemId = Guid.Parse("016a1fce-3d78-46cd-8b25-b0f911c55644"),
                    Name = "algoduck",
                    Description = "description",
                    Price = 0,
                    Purchasable = false,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },


                new PlantItem
                {
                    ItemId = Guid.Parse("19f662b6-cb89-49d7-a2f5-e87299e237fb"),
                    Name = "Christmas tree",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 4,
                    Height = 3,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("64058fa8-9ae3-435b-bbf8-c2005cad364e"),
                    Name = "birch",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 3,
                    Height = 5,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("7d819d69-51fb-4528-92ff-26ead5cf825b"),
                    Name = "birch #2",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 2,
                    Height = 4,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("ee46cf1b-0609-49c6-9619-c6b2a6199e44"),
                    Name = "rose bush",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 2,
                    Height = 2,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("2cee9ac1-78f5-45ad-ae7e-f00610c9911e"),
                    Name = "violet bush",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 5,
                    Height = 3,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("069f8ee0-96bd-4bed-bbcf-e7f76061657e"),
                    Name = "oak",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 4,
                    Height = 3,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("561c0c66-114d-4cb5-b9ba-735ff20ba9dd"),
                    Name = "iris",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 1,
                    Height = 1,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("0557da00-35bc-47ff-a431-818ffa3ac4ef"),
                    Name = "daffodil",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 1,
                    Height = 1,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                },
                new PlantItem
                {
                    ItemId = Guid.Parse("f56993d3-2052-4ef4-a48c-392025f10721"),
                    Name = "big ass tree",
                    Description = "description",
                    Price = 500,
                    Purchasable = true,
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"),
                    Width = 6,
                    Height = 4,
                    CreatedById = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                    CreatedAt = DateTime.UtcNow,
                }
            };

            await _context.Items.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }

    /* TODO: Lowkey don't know whether we should store this in a dict table. */
    private async Task SeedEditorThemes()
    {
        if (!await _context.EditorThemes.AnyAsync())
        {
            await _context.EditorThemes.AddRangeAsync(new EditorTheme
            {
                ThemeName = "vs-dark",
                EditorThemeId = Guid.Parse("276cc32e-a0bd-408e-b6f0-0f4e3ff80796"),
            }, new EditorTheme
            {
                ThemeName = "vs",
                EditorThemeId = Guid.Parse("07c5d143-7e8f-439a-acd4-695b9ecc0143")
            }, new EditorTheme
            {
                ThemeName = "dracula",
                EditorThemeId = Guid.Parse("535fa22e-998d-4e03-aab5-a10a681ab8f6")
            });
            await _context.SaveChangesAsync();
        }
    }

private async Task SeedProblems()
{
    if (!await _context.Problems.AnyAsync())
    {
        var problems = new List<Problem>
        {
            new Problem
            {
                ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                ProblemTitle = "Linked List Cycle Detection",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"),
                ProblemTitle = "Reverse Linked List",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"),
                ProblemTitle = "Valid Parentheses",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            
            new Problem
            {
                ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                ProblemTitle = "Two Sum",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("5c721265-24a9-4ed8-8214-f415d4a9bede"),
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"),
                ProblemTitle = "Contains Duplicate",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("5c721265-24a9-4ed8-8214-f415d4a9bede"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                ProblemTitle = "Maximum Subarray",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("5c721265-24a9-4ed8-8214-f415d4a9bede"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            
           
            new Problem
            {
                ProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                ProblemTitle = "Valid Anagram",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("3b676e51-aa3c-40d5-af15-1cfe04b52c37"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                ProblemTitle = "Reverse String",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("3b676e51-aa3c-40d5-af15-1cfe04b52c37"), 
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            },
            new Problem
            {
                ProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d"),
                ProblemTitle = "Longest Common Prefix",
                CreatedAt = DateTime.UtcNow,
                Status = ProblemStatus.Verified,
                CategoryId = Guid.Parse("3b676e51-aa3c-40d5-af15-1cfe04b52c37"),
                CreatedByUserId = Guid.Parse("a88e81ec-9a43-480c-8568-e9e3ceb3ba45"),
                DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
            }
        };

        List<ProblemS3PartialTemplate> templates =
        [
            new()
            {
                ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                Template =
                    "public class Main {\n    private static class Node {\n        int data;\n        Node next;\n        Node prev;\n\n        public Node(int data) {\n            this.data = data;\n            this.next = null;\n            this.prev = null;\n        }\n    }\n\n    public static boolean hasCycle(Node start) {\n        // Implement the tortoise and hare algorithm here\n        return false;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"),
                Template =
                    "public class Main {\n    private static class ListNode {\n        int val;\n        ListNode next;\n\n        public ListNode(int val) {\n            this.val = val;\n            this.next = null;\n        }\n    }\n\n    public static ListNode reverseList(ListNode head) {\n        // Implement your solution here\n        return null;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"),
                Template =
                    "public class Main {\n    public static boolean isValid(String s) {\n        // Implement your solution here\n        return false;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                Template =
                    "public class Main {\n    public static int[] twoSum(int[] nums, int target) {\n        // Implement your solution here\n        return new int[] {};\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"),
                Template =
                    "public class Main {\n    public static boolean containsDuplicate(int[] nums) {\n        // Implement your solution here\n        return false;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                Template =
                    "public class Main {\n    public static int maxSubArray(int[] nums) {\n        // Implement your solution here (Kadane's algorithm)\n        return 0;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                Template =
                    "public class Main {\n    public static boolean isAnagram(String s, String t) {\n        // Implement your solution here\n        return false;\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                Template =
                    "public class Main {\n    public static void reverseString(char[] s) {\n        // Implement your solution here (in-place)\n    }\n}\n",
            },
            new()
            {
                ProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d"),
                Template =
                    "public class Main {\n    public static String longestCommonPrefix(String[] strs) {\n        // Implement your solution here\n        return \"\";\n    }\n}\n",
            }
        ];

        foreach (var template in templates)
        {
            var objectPath = $"problems/{template.ProblemId}/template.xml";
            var objectExistsResult = await _s3Client.ObjectExistsAsync(objectPath);
            if (objectExistsResult is { IsOk: true, AsOk: false })
            {
                await _s3Client.PostXmlObjectAsync(objectPath, template);
            }
        }

        List<ProblemS3PartialInfo> partialInfos =
        [
            new()
            {
                ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                CountryCode = SupportedLanguage.En,
                Title = "Linked List Cycle Detection",
                Description =
                    "<p>In many applications, linked lists are used to represent dynamic data structures.<br>However, faulty logic or unintended pointer manipulations can sometimes cause a <strong>cycle</strong> to appear in the list, meaning that traversal never reaches a <code>null</code> terminator.</p><p>Your task is to implement a cycle detection algorithm for a <strong>doubly linked list</strong>. Specifically, you should:</p><ol><li><p><strong>Define a </strong><code>Node</code><strong> class</strong></p></li></ol><ul><li><p>Contains an integer value</p></li><li><p>Has both <code>next</code> and <code>prev</code> references</p></li></ul><ol start=\"2\"><li><p><strong>Implement a method </strong><code>hasCycle(Node start)</code></p></li></ol><ul><li><p>Determines whether a cycle exists starting from the provided node</p></li></ul><ol start=\"3\"><li><p><strong>Use Floyd's Tortoise and Hare algorithm</strong></p></li></ol><ul><li><p>A classic two-pointer technique</p></li><li><p>Detects the cycle efficiently in <strong>O(n) time</strong> and <strong>O(1) space</strong><br>A correct solution should be able to identify both the <strong>presence and absence of cycles</strong> for lists of varying sizes.</p></li></ul><h3><strong>Edge Cases to Consider</strong></h3><ul><li><p>Empty list (<code>null</code> start node)</p></li><li><p>Single-node list without a cycle</p></li><li><p>Single-node list that links to itself</p></li></ul><p></p>"
            },
            new()
            {
                ProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"),
                CountryCode = SupportedLanguage.En,
                Title = "Reverse Linked List",
                Description =
                    "<p>Given the <code>head</code> of a singly linked list, reverse the list, and return <em>the reversed list</em>.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> head = [1, 2, 3, 4, 5]<br><strong>Output:</strong> [5, 4, 3, 2, 1]</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> head = [1, 2]<br><strong>Output:</strong> [2, 1]</p><h3><strong>Example 3</strong></h3><p><strong>Input:</strong> head = []<br><strong>Output:</strong> []</p><h3><strong>Constraints</strong></h3><ul><li><p>The number of nodes in the list is in the range [0, 5000]</p></li><li><p>-5000 &lt;= Node.val &lt;= 5000</p></li></ul><h3><strong>Follow-up</strong></h3><p>A linked list can be reversed either iteratively or recursively. Could you implement both?</p>"
            },
            new()
            {
                ProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"),
                CountryCode = SupportedLanguage.En,
                Title = "Valid Parentheses",
                Description =
                    "<p>Given a string <code>s</code> containing just the characters <code>'('</code>, <code>')'</code>, <code>'{'</code>, <code>'}'</code>, <code>'['</code> and <code>']'</code>, determine if the input string is valid.</p><p>An input string is valid if:</p><ol><li><p>Open brackets must be closed by the same type of brackets.</p></li><li><p>Open brackets must be closed in the correct order.</p></li><li><p>Every close bracket has a corresponding open bracket of the same type.</p></li></ol><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> s = \"()\"<br><strong>Output:</strong> true</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> s = \"()[]{}\"<br><strong>Output:</strong> true</p><h3><strong>Example 3</strong></h3><p><strong>Input:</strong> s = \"(]\"<br><strong>Output:</strong> false</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= s.length &lt;= 10^4</p></li><li><p>s consists of parentheses only '()[]{}'</p></li></ul>"
            },
            new()
            {
                ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                CountryCode = SupportedLanguage.En,
                Title = "Two Sum",
                Description =
                    "<p>Given an array of integers <code>nums</code> and an integer <code>target</code>, return the <strong>indices</strong> of the two numbers that add up to <code>target</code>.</p><p>You may assume that each input has <strong>exactly one solution</strong>, and you <strong>cannot use the same element twice</strong>.</p><p>You can return the answer in any order.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> nums = [2, 7, 11, 15], target = 9<br><strong>Output:</strong> [0, 1]<br><strong>Explanation:</strong> nums[0] + nums[1] = 2 + 7 = 9</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> nums = [3, 2, 4], target = 6<br><strong>Output:</strong> [1, 2]<br><strong>Explanation:</strong> nums[1] + nums[2] = 2 + 4 = 6</p><h3><strong>Constraints</strong></h3><ul><li><p>2 &lt;= nums.length &lt;= 10^4</p></li><li><p>-10^9 &lt;= nums[i] &lt;= 10^9</p></li><li><p>-10^9 &lt;= target &lt;= 10^9</p></li><li><p>Only one valid answer exists</p></li></ul><h3><strong>Follow-up</strong></h3><p>Can you solve it in less than O(n²) time complexity?</p>"
            },
            new()
            {
                ProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"),
                CountryCode = SupportedLanguage.En,
                Title = "Contains Duplicate",
                Description =
                    "<p>Given an integer array <code>nums</code>, return <code>true</code> if any value appears <strong>at least twice</strong> in the array, and return <code>false</code> if every element is distinct.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> nums = [1, 2, 3, 1]<br><strong>Output:</strong> true<br><strong>Explanation:</strong> The element 1 occurs at indices 0 and 3.</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> nums = [1, 2, 3, 4]<br><strong>Output:</strong> false<br><strong>Explanation:</strong> All elements are distinct.</p><h3><strong>Example 3</strong></h3><p><strong>Input:</strong> nums = [1, 1, 1, 3, 3, 4, 3, 2, 4, 2]<br><strong>Output:</strong> true</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= nums.length &lt;= 10^5</p></li><li><p>-10^9 &lt;= nums[i] &lt;= 10^9</p></li></ul>"
            },
            new()
            {
                ProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                CountryCode = SupportedLanguage.En,
                Title = "Maximum Subarray",
                Description =
                    "<p>Given an integer array <code>nums</code>, find the subarray with the largest sum, and return <em>its sum</em>.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> nums = [-2, 1, -3, 4, -1, 2, 1, -5, 4]<br><strong>Output:</strong> 6<br><strong>Explanation:</strong> The subarray [4, -1, 2, 1] has the largest sum 6.</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> nums = [1]<br><strong>Output:</strong> 1<br><strong>Explanation:</strong> The subarray [1] has the largest sum 1.</p><h3><strong>Example 3</strong></h3><p><strong>Input:</strong> nums = [5, 4, -1, 7, 8]<br><strong>Output:</strong> 23<br><strong>Explanation:</strong> The subarray [5, 4, -1, 7, 8] has the largest sum 23.</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= nums.length &lt;= 10^5</p></li><li><p>-10^4 &lt;= nums[i] &lt;= 10^4</p></li></ul><h3><strong>Follow-up</strong></h3><p>If you have figured out the O(n) solution, try coding another solution using the divide and conquer approach, which is more subtle.</p>"
            },
            new()
            {
                ProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                CountryCode = SupportedLanguage.En,
                Title = "Valid Anagram",
                Description =
                    "<p>Given two strings <code>s</code> and <code>t</code>, return <code>true</code> if <code>t</code> is an <strong>anagram</strong> of <code>s</code>, and <code>false</code> otherwise.</p><p>An <strong>Anagram</strong> is a word or phrase formed by rearranging the letters of a different word or phrase, typically using all the original letters exactly once.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> s = \"anagram\", t = \"nagaram\"<br><strong>Output:</strong> true</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> s = \"rat\", t = \"car\"<br><strong>Output:</strong> false</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= s.length, t.length &lt;= 5 * 10^4</p></li><li><p>s and t consist of lowercase English letters</p></li></ul><h3><strong>Follow-up</strong></h3><p>What if the inputs contain Unicode characters? How would you adapt your solution to such a case?</p>"
            },
            new()
            {
                ProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                CountryCode = SupportedLanguage.En,
                Title = "Reverse String",
                Description =
                    "<p>Write a function that reverses a string. The input string is given as an array of characters <code>s</code>.</p><p>You must do this by modifying the input array <strong>in-place</strong> with <code>O(1)</code> extra memory.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> s = ['h', 'e', 'l', 'l', 'o']<br><strong>Output:</strong> ['o', 'l', 'l', 'e', 'h']</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> s = ['H', 'a', 'n', 'n', 'a', 'h']<br><strong>Output:</strong> ['h', 'a', 'n', 'n', 'a', 'H']</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= s.length &lt;= 10^5</p></li><li><p>s[i] is a printable ASCII character</p></li></ul>"
            },
            new()
            {
                ProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d"),
                CountryCode = SupportedLanguage.En,
                Title = "Longest Common Prefix",
                Description =
                    "<p>Write a function to find the longest common prefix string amongst an array of strings.</p><p>If there is no common prefix, return an empty string <code>\"\"</code>.</p><h3><strong>Example 1</strong></h3><p><strong>Input:</strong> strs = [\"flower\", \"flow\", \"flight\"]<br><strong>Output:</strong> \"fl\"</p><h3><strong>Example 2</strong></h3><p><strong>Input:</strong> strs = [\"dog\", \"racecar\", \"car\"]<br><strong>Output:</strong> \"\"<br><strong>Explanation:</strong> There is no common prefix among the input strings.</p><h3><strong>Constraints</strong></h3><ul><li><p>1 &lt;= strs.length &lt;= 200</p></li><li><p>0 &lt;= strs[i].length &lt;= 200</p></li><li><p>strs[i] consists of only lowercase English letters</p></li></ul>"
            }
        ];

        foreach (var info in partialInfos)
        {
            var objectPath =
                $"problems/{info.ProblemId}/infos/{info.CountryCode.GetDisplayName().ToLowerInvariant()}.xml";
            var objectExistsResult = await _s3Client.ObjectExistsAsync(objectPath);
            if (objectExistsResult is { IsOk: true, AsOk: false })
            {
                await _s3Client.PostXmlObjectAsync(objectPath, info);
            }
        }

        await _context.Problems.AddRangeAsync(problems);
        await _context.SaveChangesAsync();
    }
}

private async Task SeedTestCases()
{
    if (!await _context.TestCases.AnyAsync())
    {
        List<TestCase> testCases =
        [
            new TestCase
            {
                TestCaseId = Guid.Parse("7a2264fa-b7a2-4250-ac4b-a868f746c978"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.hasCycle",
                IsPublic = true,
                Display = "Linear list: 1 -> 2 -> 3 -> 4 -> null",
                DisplayRes = "false (no cycle)",
                ArrangeVariableCount = 5,
                ProblemProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("2ed6b7ae-4dd0-4c26-84ee-ce849dd9ce13"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.hasCycle",
                IsPublic = true,
                ArrangeVariableCount = 5,
                Display = "Cyclic list: 1 -> 2 -> 3 -> 4 -> (back to 2)",
                DisplayRes = "true (cycle detected)",
                ProblemProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.hasCycle",
                IsPublic = false,
                Display = "Two-node cycle: 10 <-> 20",
                ArrangeVariableCount = 3,
                DisplayRes = "true (cycle detected)",
                ProblemProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("acb062e7-922f-4ed0-b86c-ac2562a4b959"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.hasCycle",
                IsPublic = false,
                ArrangeVariableCount = 2,
                Display = "Single node: 5 -> null",
                DisplayRes = "false (no cycle)",
                ProblemProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseList",
                IsPublic = true,
                Display = "head = [1, 2, 3, 4, 5]",
                DisplayRes = "[5, 4, 3, 2, 1]",
                ArrangeVariableCount = 11,
                ProblemProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseList",
                IsPublic = true,
                Display = "head = [1, 2]",
                DisplayRes = "[2, 1]",
                ArrangeVariableCount = 5, 
                ProblemProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseList",
                IsPublic = false,
                Display = "head = []",
                DisplayRes = "[]",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseList",
                IsPublic = false,
                Display = "head = [1]",
                DisplayRes = "[1]",
                ArrangeVariableCount = 3, 
                ProblemProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isValid",
                IsPublic = true,
                Display = "s = \"()\"",
                DisplayRes = "true",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("66666666-6666-4666-8666-666666666666"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isValid",
                IsPublic = true,
                Display = "s = \"()[]{}\"",
                DisplayRes = "true",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("77777777-7777-4777-8777-777777777777"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isValid",
                IsPublic = false,
                Display = "s = \"(]\"",
                DisplayRes = "false",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("88888888-8888-4888-8888-888888888888"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isValid",
                IsPublic = false,
                Display = "s = \"([)]\"",
                DisplayRes = "false",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("6b9c1f59-2700-4840-83ae-9a7ab9253b2e"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.twoSum",
                IsPublic = true,
                Display = "nums = [2, 7, 11, 15], target = 9",
                ArrangeVariableCount = 3,
                DisplayRes = "[0, 1]",
                ProblemProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("94c73084-9ac2-47fe-ab0b-536bb398e2fb"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.twoSum",
                IsPublic = true,
                Display = "nums = [3, 2, 4], target = 6",
                ArrangeVariableCount = 3,
                DisplayRes = "[1, 2]",
                ProblemProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("533c0f81-df26-4a83-b72f-676b49dfb93a"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.twoSum",
                IsPublic = false,
                Display = "nums = [3, 3], target = 6",
                ArrangeVariableCount = 3,
                DisplayRes = "[0, 1]",
                ProblemProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("c18ddef1-6910-4445-bb4b-41f5a1580f72"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.twoSum",
                IsPublic = false,
                Display = "nums = [1, 5, 3, 7, 9, 2], target = 10",
                ArrangeVariableCount = 3,
                DisplayRes = "[2, 3]",
                ProblemProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("99999999-9999-4999-8999-999999999999"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.containsDuplicate",
                IsPublic = true,
                Display = "nums = [1, 2, 3, 1]",
                DisplayRes = "true",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.containsDuplicate",
                IsPublic = true,
                Display = "nums = [1, 2, 3, 4]",
                DisplayRes = "false",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.containsDuplicate",
                IsPublic = false,
                Display = "nums = [1, 1, 1, 3, 3, 4, 3, 2, 4, 2]",
                DisplayRes = "true",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.containsDuplicate",
                IsPublic = false,
                Display = "nums = []",
                DisplayRes = "false",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.maxSubArray",
                IsPublic = true,
                Display = "nums = [-2, 1, -3, 4, -1, 2, 1, -5, 4]",
                DisplayRes = "6",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.maxSubArray",
                IsPublic = true,
                Display = "nums = [1]",
                DisplayRes = "1",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("ffffffff-ffff-4fff-8fff-ffffffffffff"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.maxSubArray",
                IsPublic = false,
                Display = "nums = [5, 4, -1, 7, 8]",
                DisplayRes = "23",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("01234567-0123-4012-8012-012345670123"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.maxSubArray",
                IsPublic = false,
                Display = "nums = [-1]",
                DisplayRes = "-1",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("12345678-1234-4123-8123-123456781234"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isAnagram",
                IsPublic = true,
                Display = "s = \"anagram\", t = \"nagaram\"",
                DisplayRes = "true",
                ArrangeVariableCount = 3,
                ProblemProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("23456789-2345-4234-8234-234567892345"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isAnagram",
                IsPublic = true,
                Display = "s = \"rat\", t = \"car\"",
                DisplayRes = "false",
                ArrangeVariableCount = 3,
                ProblemProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("3456789a-3456-4345-8345-3456789a3456"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isAnagram",
                IsPublic = false,
                Display = "s = \"a\", t = \"a\"",
                DisplayRes = "true",
                ArrangeVariableCount = 3,
                ProblemProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("456789ab-4567-4456-8456-456789ab4567"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.isAnagram",
                IsPublic = false,
                Display = "s = \"ab\", t = \"a\"",
                DisplayRes = "false",
                ArrangeVariableCount = 3,
                ProblemProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b")
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("56789abc-5678-4567-8567-56789abc5678"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseString",
                IsPublic = true,
                OrderMatters = true,
                Display = "s = ['h', 'e', 'l', 'l', 'o']",
                DisplayRes = "['o', 'l', 'l', 'e', 'h']",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                InPlace = true
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("6789abcd-6789-4678-8678-6789abcd6789"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseString",
                IsPublic = true,
                OrderMatters = true,
                Display = "s = ['H', 'a', 'n', 'n', 'a', 'h']",
                DisplayRes = "['h', 'a', 'n', 'n', 'a', 'H']",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                InPlace = true
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("789abcde-789a-4789-8789-789abcde789a"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseString",
                IsPublic = false,
                OrderMatters = true,
                Display = "s = ['a']",
                DisplayRes = "['a']",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                InPlace = true
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("89abcdef-89ab-489a-889a-89abcdef89ab"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.reverseString",
                IsPublic = false,
                OrderMatters = true,
                Display = "s = ['a', 'b']",
                DisplayRes = "['b', 'a']",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                InPlace = true
            },
            
            new TestCase
            {
                TestCaseId = Guid.Parse("9abcdef0-9abc-49ab-89ab-9abcdef09abc"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.longestCommonPrefix",
                IsPublic = true,
                Display = "strs = [\"flower\", \"flow\", \"flight\"]",
                DisplayRes = "\"fl\"",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("abcdef01-abcd-4abc-8abc-abcdef01abcd"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.longestCommonPrefix",
                IsPublic = true,
                Display = "strs = [\"dog\", \"racecar\", \"car\"]",
                DisplayRes = "\"\"",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("bcdef012-bcde-4bcd-8bcd-bcdef012bcde"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.longestCommonPrefix",
                IsPublic = false,
                Display = "strs = [\"a\"]",
                DisplayRes = "\"a\"",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d")
            },
            new TestCase
            {
                TestCaseId = Guid.Parse("cdef0123-cdef-4cde-8cde-cdef0123cdef"),
                CallFunc = "${ENTRYPOINT_CLASS_NAME}.longestCommonPrefix",
                IsPublic = false,
                Display = "strs = [\"cir\", \"car\"]",
                DisplayRes = "\"c\"",
                ArrangeVariableCount = 2,
                ProblemProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d")
            }
        ];

        List<TestCaseS3WrapperObject> testCaseS3Partials =
        [
            new()
            {
                ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("7a2264fa-b7a2-4250-ac4b-a868f746c978"),
                        Expected = "{tc_0_var_4}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_2} = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_3} = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        {tc_0_var_0}.next = {tc_0_var_1};\n        {tc_0_var_1}.prev = {tc_0_var_0};\n        {tc_0_var_1}.next = {tc_0_var_2};\n        {tc_0_var_2}.prev = {tc_0_var_1};\n        {tc_0_var_2}.next = {tc_0_var_3};\n        {tc_0_var_3}.prev = {tc_0_var_2};\n        boolean {tc_0_var_4} = false;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("2ed6b7ae-4dd0-4c26-84ee-ce849dd9ce13"),
                        Expected = "{tc_1_var_4}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_2} = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_3} = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        {tc_1_var_0}.next = {tc_1_var_1};\n        {tc_1_var_1}.prev = {tc_1_var_0};\n        {tc_1_var_1}.next = {tc_1_var_2};\n        {tc_1_var_2}.prev = {tc_1_var_1};\n        {tc_1_var_2}.next = {tc_1_var_3};\n        {tc_1_var_3}.prev = {tc_1_var_2};\n        {tc_1_var_3}.next = {tc_1_var_1};\n        boolean {tc_1_var_4} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                        Expected = "{tc_2_var_2}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.Node {tc_2_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(10);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_2_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(20);\n        {tc_2_var_0}.next = {tc_2_var_1};\n        {tc_2_var_1}.prev = {tc_2_var_0};\n        {tc_2_var_1}.next = {tc_2_var_0};\n        boolean {tc_2_var_2} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("acb062e7-922f-4ed0-b86c-ac2562a4b959"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.Node {tc_3_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(5);\n        boolean {tc_3_var_1} = false;"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                        Expected = "{tc_0_var_10}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_0} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_1} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(2);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_2} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(3);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_3} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(4);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_4} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(5);\n        {tc_0_var_0}.next = {tc_0_var_1};\n        {tc_0_var_1}.next = {tc_0_var_2};\n        {tc_0_var_2}.next = {tc_0_var_3};\n        {tc_0_var_3}.next = {tc_0_var_4};\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_5} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(5);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_6} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(4);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_7} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(3);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_8} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(2);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_9} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        {tc_0_var_5}.next = {tc_0_var_6};\n        {tc_0_var_6}.next = {tc_0_var_7};\n        {tc_0_var_7}.next = {tc_0_var_8};\n        {tc_0_var_8}.next = {tc_0_var_9};\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_0_var_10} = {tc_0_var_5};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                        Expected = "{tc_1_var_4}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.ListNode {tc_1_var_0} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_1_var_1} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(2);\n        {tc_1_var_0}.next = {tc_1_var_1};\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_1_var_2} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(2);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_1_var_3} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        {tc_1_var_2}.next = {tc_1_var_3};\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_1_var_4} = {tc_1_var_2};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.ListNode {tc_2_var_0} = null;\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_2_var_1} = null;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                        Expected = "{tc_3_var_2}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup =
                            "${ENTRYPOINT_CLASS_NAME}.ListNode {tc_3_var_0} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_3_var_1} = new ${ENTRYPOINT_CLASS_NAME}.ListNode(1);\n        ${ENTRYPOINT_CLASS_NAME}.ListNode {tc_3_var_2} = {tc_3_var_1};"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                        Expected = "{tc_0_var_1}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup = "String {tc_0_var_0} = \"()\";\n        boolean {tc_0_var_1} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("66666666-6666-4666-8666-666666666666"),
                        Expected = "{tc_1_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup = "String {tc_1_var_0} = \"()[]{}\";\n        boolean {tc_1_var_1} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("77777777-7777-4777-8777-777777777777"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup = "String {tc_2_var_0} = \"(]\";\n        boolean {tc_2_var_1} = false;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("88888888-8888-4888-8888-888888888888"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup = "String {tc_3_var_0} = \"([)]\";\n        boolean {tc_3_var_1} = false;"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("6b9c1f59-2700-4840-83ae-9a7ab9253b2e"),
                        Expected = "{tc_0_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_1}"
                        }],
                        Setup =
                            "int[] {tc_0_var_0} = new int[] {2, 7, 11, 15};\n        int {tc_0_var_1} = 9;\n        int[] {tc_0_var_2} = new int[] {0, 1};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("94c73084-9ac2-47fe-ab0b-536bb398e2fb"),
                        Expected = "{tc_1_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_1}"
                        }],
                        Setup =
                            "int[] {tc_1_var_0} = new int[] {3, 2, 4};\n        int {tc_1_var_1} = 6;\n        int[] {tc_1_var_2} = new int[] {1, 2};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("533c0f81-df26-4a83-b72f-676b49dfb93a"),
                        Expected = "{tc_2_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_1}"
                        }],
                        Setup =
                            "int[] {tc_2_var_0} = new int[] {3, 3};\n        int {tc_2_var_1} = 6;\n        int[] {tc_2_var_2} = new int[] {0, 1};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("c18ddef1-6910-4445-bb4b-41f5a1580f72"),
                        Expected = "{tc_3_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_1}"
                        }],
                        Setup =
                            "int[] {tc_3_var_0} = new int[] {1, 5, 3, 7, 9, 2};\n        int {tc_3_var_1} = 10;\n        int[] {tc_3_var_2} = new int[] {2, 3};"
                    }
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("99999999-9999-4999-8999-999999999999"),
                        Expected = "{tc_0_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup = "int[] {tc_0_var_0} = new int[] {1, 2, 3, 1};\n        boolean {tc_0_var_1} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                        Expected = "{tc_1_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup = "int[] {tc_1_var_0} = new int[] {1, 2, 3, 4};\n        boolean {tc_1_var_1} = false;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup = "int[] {tc_2_var_0} = new int[] {1, 1, 1, 3, 3, 4, 3, 2, 4, 2};\n        boolean {tc_2_var_1} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup = "int[] {tc_3_var_0} = new int[] {};\n        boolean {tc_3_var_1} = false;"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
                        Expected = "{tc_0_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup = "int[] {tc_0_var_0} = new int[] {-2, 1, -3, 4, -1, 2, 1, -5, 4};\n        int {tc_0_var_1} = 6;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
                        Expected = "{tc_1_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup = "int[] {tc_1_var_0} = new int[] {1};\n        int {tc_1_var_1} = 1;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("ffffffff-ffff-4fff-8fff-ffffffffffff"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup = "int[] {tc_2_var_0} = new int[] {5, 4, -1, 7, 8};\n        int {tc_2_var_1} = 23;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("01234567-0123-4012-8012-012345670123"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup = "int[] {tc_3_var_0} = new int[] {-1};\n        int {tc_3_var_1} = -1;"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("12345678-1234-4123-8123-123456781234"),
                        Expected = "{tc_0_var_2}",
                        Call = [new TestCaseCallArg
                        {
                            VarName = "{tc_0_var_0}"
                        }, new TestCaseCallArg
                        {
                            VarName = "{tc_0_var_1}"
                        }],
                        Setup = "String {tc_0_var_0} = \"anagram\";\n        String {tc_0_var_1} = \"nagaram\";\n        boolean {tc_0_var_2} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("23456789-2345-4234-8234-234567892345"),
                        Expected = "{tc_1_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_1}"
                        }],
                        Setup = "String {tc_1_var_0} = \"rat\";\n        String {tc_1_var_1} = \"car\";\n        boolean {tc_1_var_2} = false;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("3456789a-3456-4345-8345-3456789a3456"),
                        Expected = "{tc_2_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_1}"
                        }],
                        Setup = "String {tc_2_var_0} = \"a\";\n        String {tc_2_var_1} = \"a\";\n        boolean {tc_2_var_2} = true;"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("456789ab-4567-4456-8456-456789ab4567"),
                        Expected = "{tc_3_var_2}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }, new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_1}"
                        }],
                        Setup = "String {tc_3_var_0} = \"ab\";\n        String {tc_3_var_1} = \"a\";\n        boolean {tc_3_var_2} = false;"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b2c-4d5e6f7a8b9c"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("56789abc-5678-4567-8567-56789abc5678"),
                        Expected = "{tc_0_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_0}",
                            IsMutated = true
                        }],
                        Setup = "char[] {tc_0_var_0} = new char[] {'h', 'e', 'l', 'l', 'o'};\n        char[] {tc_0_var_1} = new char[] {'o', 'l', 'l', 'e', 'h'};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("6789abcd-6789-4678-8678-6789abcd6789"),
                        Expected = "{tc_1_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}",
                            IsMutated = true
                        }],
                        Setup = "char[] {tc_1_var_0} = new char[] {'H', 'a', 'n', 'n', 'a', 'h'};\n        char[] {tc_1_var_1} = new char[] {'h', 'a', 'n', 'n', 'a', 'H'};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("789abcde-789a-4789-8789-789abcde789a"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}",
                            IsMutated = true
                        }],
                        Setup = "char[] {tc_2_var_0} = new char[] {'a'};\n        char[] {tc_2_var_1} = new char[] {'a'};"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("89abcdef-89ab-489a-889a-89abcdef89ab"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}",
                            IsMutated = true
                        }],
                        Setup = "char[] {tc_3_var_0} = new char[] {'a', 'b'};\n        char[] {tc_3_var_1} = new char[] {'b', 'a'};"
                    },
                ]
            },
            new()
            {
                ProblemId = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c3d-5e6f7a8b9c0d"),
                TestCases =
                [
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("9abcdef0-9abc-49ab-89ab-9abcdef09abc"),
                        Expected = "{tc_0_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_0_var_0}"
                        }],
                        Setup = "String[] {tc_0_var_0} = new String[] {\"flower\", \"flow\", \"flight\"};\n        String {tc_0_var_1} = \"fl\";"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("abcdef01-abcd-4abc-8abc-abcdef01abcd"),
                        Expected = "{tc_1_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_1_var_0}"
                        }],
                        Setup = "String[] {tc_1_var_0} = new String[] {\"dog\", \"racecar\", \"car\"};\n        String {tc_1_var_1} = \"\";"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("bcdef012-bcde-4bcd-8bcd-bcdef012bcde"),
                        Expected = "{tc_2_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_2_var_0}"
                        }],
                        Setup = "String[] {tc_2_var_0} = new String[] {\"a\"};\n        String {tc_2_var_1} = \"a\";"
                    },
                    new TestCaseS3Partial
                    {
                        TestCaseId = Guid.Parse("cdef0123-cdef-4cde-8cde-cdef0123cdef"),
                        Expected = "{tc_3_var_1}",
                        Call = [new TestCaseCallArg()
                        {
                            VarName = "{tc_3_var_0}"
                        }],
                        Setup = "String[] {tc_3_var_0} = new String[] {\"cir\", \"car\"};\n        String {tc_3_var_1} = \"c\";"
                    },
                ]
            }
        ];

        foreach (var testCaseS3Partial in testCaseS3Partials)
        {
            var objectPath = $"problems/{testCaseS3Partial.ProblemId}/test-cases.xml";
            var objectExistsResult = await _s3Client.ObjectExistsAsync(objectPath);
            if (objectExistsResult is { IsOk: true, AsOk: false })
            {
                var res = await _s3Client.PostXmlObjectAsync(objectPath, testCaseS3Partial);
            }
        }

        await _context.TestCases.AddRangeAsync(testCases);
        await _context.SaveChangesAsync();
    }
}
}