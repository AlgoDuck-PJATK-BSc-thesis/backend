using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Shared.Utilities;

public class DataSeedingService(
    ApplicationCommandDbContext context,
    IAwsS3Client s3Client,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IDefaultDuckService defaultDuckService)
{
    public async Task SeedDataAsync()
    {
        await SeedRolesAsync();
        await SeedSeededUsersAsync();
        await SeedRarities();
        await SeedCategories();
        await SeedDifficulties();
        await SeedItems();
        await EnsureDefaultsForSeededUsersAsync();
        await SeedProblems();
        await SeedTestCases();
        await SeedEditorThemes();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = ["admin", "user"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }
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
        await defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(
            Guid.Parse("b3c38fed-69c5-4063-9d54-7cb4199dfdab"),
            CancellationToken.None);
    }

    private async Task EnsureUserWithRoleAsync(string username, string email, string password, string role, Guid? userId = null)
    {
        var user = await userManager.FindByNameAsync(username);

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
            };

            var createResult = await userManager.CreateAsync(user, password);
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
                var setEmailResult = await userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to update email for '{username}': {IdentityErrorsToString(setEmailResult)}");
                }
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to confirm email for '{username}': {IdentityErrorsToString(updateResult)}");
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
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
        if (!await context.Rarities.AnyAsync())
        {
            var rarities = new List<Rarity>
            {
                new Rarity { RarityId = Guid.Parse("016a1fce-3d78-46cd-8b25-b0f911c55642"), RarityName = "COMMON" },
                new Rarity { RarityId = Guid.Parse("ea1da060-6add-423e-a5bc-cc81d31f98ac"), RarityName = "UNCOMMON" },
                new Rarity { RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a"), RarityName = "RARE" },
                new Rarity { RarityId = Guid.Parse("c86c74ea-109a-4402-8606-c653d117edf2"), RarityName = "EPIC" },
                new Rarity { RarityId = Guid.Parse("f3b9d57f-0c2f-444e-938f-57fd2782bf0a"), RarityName = "LEGENDARY" }
            };

            await context.Rarities.AddRangeAsync(rarities);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedCategories()
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"), CategoryName = "test category 4"
                },
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedDifficulties()
    {
        if (!await context.Difficulties.AnyAsync())
        {
            await context.Difficulties.AddRangeAsync(new Difficulty
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
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedItems()
    {
        if (!await context.Items.AnyAsync())
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

            await context.Items.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }

    /* TODO: Lowkey don't know whether we should store this in a dict table. */
    private async Task SeedEditorThemes()
    {
        if (!await context.EditorThemes.AnyAsync())
        {
            await context.EditorThemes.AddRangeAsync(new EditorTheme
            {
                ThemeName = "vs-dark",
                EditorThemeId = Guid.Parse("276cc32e-a0bd-408e-b6f0-0f4e3ff80796")
            }, new EditorTheme
            {
                ThemeName = "vs",
                EditorThemeId = Guid.Parse("07c5d143-7e8f-439a-acd4-695b9ecc0143")
            }, new EditorTheme
            {
                ThemeName = "dracula",
                EditorThemeId = Guid.Parse("535fa22e-998d-4e03-aab5-a10a681ab8f6")
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedProblems()
    {
        if (!await context.Problems.AnyAsync())
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
                    ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                    ProblemTitle = "Two Sum",
                    CreatedAt = DateTime.UtcNow,
                    Status = ProblemStatus.Verified,
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
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
                    ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                    Template =
                        "public class Main {\n    public static int[] twoSum(int[] nums, int target) {\n        // Implement your solution here\n        return new int[] {};\n    }\n}\n",
                }
            ];

            foreach (var template in templates)
            {
                var objectPath = $"problems/{template.ProblemId}/template.xml";
                var objectExistsResult = await s3Client.ObjectExistsAsync(objectPath);
                if (objectExistsResult is { IsOk: true, AsOk: false })
                {
                    await s3Client.PostXmlObjectAsync(objectPath,
                        template);
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
                        "In many applications, linked lists are used to represent dynamic data structures.  \nHowever, faulty logic or unintended pointer manipulations can sometimes cause a **cycle** to appear in the list, meaning that traversal never reaches a `null` terminator.  \n\nYour task is to implement a cycle detection algorithm for a **doubly linked list**. Specifically, you should: \n1. **Define a `Node` class**  \n- Contains an integer value  \n- Has both `next` and `prev` references  \n\n2. **Implement a method `hasCycle(Node start)`**  \n- Determines whether a cycle exists starting from the provided node  \n3. **Use Floyd's Tortoise and Hare algorithm**  \n- A classic two-pointer technique  \n- Detects the cycle efficiently in **O(n) time** and **O(1) space**  \nA correct solution should be able to identify both the **presence and absence of cycles** for lists of varying sizes.  \n### Edge Cases to Consider\n- Empty list (`null` start node)  \n- Single-node list without a cycle  \n- Single-node list that links to itself"
                },
                new()
                {
                    ProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2"),
                    CountryCode = SupportedLanguage.En,
                    Title = "Two Sum",
                    Description =
                        "Given an array of integers `nums` and an integer `target`, return the **indices** of the two numbers that add up to `target`.\n\nYou may assume that each input has **exactly one solution**, and you **cannot use the same element twice**.\n\nYou can return the answer in any order.\n\n### Example 1\n**Input:** nums = [2, 7, 11, 15], target = 9  \n**Output:** [0, 1]  \n**Explanation:** nums[0] + nums[1] = 2 + 7 = 9\n\n### Example 2\n**Input:** nums = [3, 2, 4], target = 6  \n**Output:** [1, 2]  \n**Explanation:** nums[1] + nums[2] = 2 + 4 = 6\n\n### Constraints\n- 2 <= nums.length <= 10^4\n- -10^9 <= nums[i] <= 10^9\n- -10^9 <= target <= 10^9\n- Only one valid answer exists\n\n### Follow-up\nCan you solve it in less than O(n²) time complexity?"
                }
            ];

            foreach (var info in partialInfos)
            {
                var objectPath =
                    $"problems/{info.ProblemId}/infos/{info.CountryCode.GetDisplayName().ToLowerInvariant()}.xml";
                var objectExistsResult = await s3Client.ObjectExistsAsync(objectPath);
                if (objectExistsResult is { IsOk: true, AsOk: false })
                {
                    await s3Client.PostXmlObjectAsync(objectPath,
                        info);
                }
            }


            await context.Problems.AddRangeAsync(problems);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedTestCases()
    {
        if (!await context.TestCases.AnyAsync())
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
                    DisplayRes = "false (no cycle)",
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
                    DisplayRes = "[2, 4]",
                    ProblemProblemId = Guid.Parse("4263ebea-54de-437c-cf4c-b8f7e1f487f2")
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
                            Call = ["{tc_0_var_0}"],
                            Setup =
                                "${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_2} = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_0_var_3} = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        {tc_0_var_0}.next = {tc_0_var_1};\n        {tc_0_var_1}.prev = {tc_0_var_0};\n        {tc_0_var_1}.next = {tc_0_var_2};\n        {tc_0_var_2}.prev = {tc_0_var_1};\n        {tc_0_var_2}.next = {tc_0_var_3};\n        {tc_0_var_3}.prev = {tc_0_var_2};\n        boolean {tc_0_var_4} = false;"
                        },

                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("2ed6b7ae-4dd0-4c26-84ee-ce849dd9ce13"),
                            Expected = "{tc_1_var_4}",
                            Call = ["{tc_1_var_0}"],
                            Setup =
                                "${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_2} = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_1_var_3} = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        {tc_1_var_0}.next = {tc_1_var_1};\n        {tc_1_var_1}.prev = {tc_1_var_0};\n        {tc_1_var_1}.next = {tc_1_var_2};\n        {tc_1_var_2}.prev = {tc_1_var_1};\n        {tc_1_var_2}.next = {tc_1_var_3};\n        {tc_1_var_3}.prev = {tc_1_var_2};\n        {tc_1_var_3}.next = {tc_1_var_1};\n        boolean {tc_1_var_4} = true;"
                        },

                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                            Expected = "{tc_2_var_2}",
                            Call = ["{tc_2_var_0}"],
                            Setup =
                                "${ENTRYPOINT_CLASS_NAME}.Node {tc_2_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(10);\n        ${ENTRYPOINT_CLASS_NAME}.Node {tc_2_var_1} = new ${ENTRYPOINT_CLASS_NAME}.Node(20);\n        {tc_2_var_0}.next = {tc_2_var_1};\n        {tc_2_var_1}.prev = {tc_2_var_0};\n        {tc_2_var_1}.next = {tc_2_var_0};\n        boolean {tc_2_var_2} = true;"
                        },

                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("acb062e7-922f-4ed0-b86c-ac2562a4b959"),
                            Expected = "{tc_3_var_1}",
                            Call = ["{tc_3_var_0}"],
                            Setup =
                                "${ENTRYPOINT_CLASS_NAME}.Node {tc_3_var_0} = new ${ENTRYPOINT_CLASS_NAME}.Node(5);\n        boolean {tc_3_var_1} = false;"
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
                            Call = ["{tc_0_var_0}", "{tc_0_var_1}"],
                            Setup = "int[] {tc_0_var_0} = new int[] {2, 7, 11, 15};\n        int {tc_0_var_1} = 9;\n        int[] {tc_0_var_2} = new int[] {0, 1};"
                        },
                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("94c73084-9ac2-47fe-ab0b-536bb398e2fb"),
                            Expected = "{tc_1_var_2}",
                            Call = ["{tc_1_var_0}", "{tc_1_var_1}"],
                            Setup = "int[] {tc_1_var_0} = new int[] {3, 2, 4};\n        int {tc_1_var_1} = 6;\n        int[] {tc_1_var_2} = new int[] {1, 2};"
                        },
                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("533c0f81-df26-4a83-b72f-676b49dfb93a"),
                            Expected = "{tc_2_var_2}",
                            Call = ["{tc_2_var_0}", "{tc_2_var_1}"],
                            Setup = "int[] {tc_2_var_0} = new int[] {3, 3};\n        int {tc_2_var_1} = 6;\n        int[] {tc_2_var_2} = new int[] {0, 1};"
                        },
                        new TestCaseS3Partial
                        {
                            TestCaseId = Guid.Parse("c18ddef1-6910-4445-bb4b-41f5a1580f72"),
                            Expected = "{tc_3_var_2}",
                            Call = ["{tc_3_var_0}", "{tc_3_var_1}"],
                            Setup = "int[] {tc_3_var_0} = new int[] {1, 5, 3, 7, 9, 2};\n        int {tc_3_var_1} = 10;\n        int[] {tc_3_var_2} = new int[] {2, 4};"
                        }
                    ]
                }
            ];

            foreach (var testCaseS3Partial in testCaseS3Partials)
            {
                var objectPath = $"problems/{testCaseS3Partial.ProblemId}/test-cases.xml";
                var objectExistsResult = await s3Client.ObjectExistsAsync(objectPath);
                if (objectExistsResult is { IsOk: true, AsOk: false })
                {
                    await s3Client.PostXmlObjectAsync(objectPath,
                        testCaseS3Partial);
                }
            }

            await context.TestCases.AddRangeAsync(testCases);
            await context.SaveChangesAsync();
        }
    }
}