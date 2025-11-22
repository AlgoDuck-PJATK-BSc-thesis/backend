using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuckShared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Shared.Utilities;

public class DataSeedingService(
    ApplicationDbContext context,
    IAwsS3Client s3Client,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task SeedDataAsync()
    {
        await SeedRarities();
        await SeedCategories();
        await SeedDifficulties();
        await SeedLanguages();
        await SeedItems();
        await SeedProblems();
        await SeedTestCases();
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
    
    private async Task SeedTestCases()
    {

        if (!await context.TestCases.AnyAsync())
        {
            List<TestCase> testCases =
            [
                new TestCase
                {
                    TestCaseId = Guid.Parse("7a2264fa-b7a2-4250-ac4b-a868f746c978"),
                    CallFunc = "hasCycle",
                    IsPublic = true,
                    Display = "Linear list: 1 -> 2 -> 3 -> 4 -> null",
                    DisplayRes = "false (no cycle)",
                    ProblemProblemId = Guid.Parse("63060846-b7e7-4584-8b16-099e0cd0ff0c")
                },
                new TestCase
                {
                    TestCaseId = Guid.Parse("2ed6b7ae-4dd0-4c26-84ee-ce849dd9ce13"),
                    CallFunc = "hasCycle",
                    IsPublic = true,
                    Display = "Cyclic list: 1 -> 2 -> 3 -> 4 -> (back to 2)",
                    DisplayRes = "false (no cycle)",
                    ProblemProblemId = Guid.Parse("63060846-b7e7-4584-8b16-099e0cd0ff0c")
                },
                new TestCase
                {
                    TestCaseId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                    CallFunc = "hasCycle",
                    IsPublic = false,
                    Display = "Two-node cycle: 10 <-> 20",
                    DisplayRes = "true (cycle detected)",
                    ProblemProblemId = Guid.Parse("63060846-b7e7-4584-8b16-099e0cd0ff0c")
                },
                new TestCase
                {
                    TestCaseId = Guid.Parse("acb062e7-922f-4ed0-b86c-ac2562a4b959"),
                    CallFunc = "hasCycle",
                    IsPublic = false,
                    Display = "Single node: 5 -> null",
                    DisplayRes = "false (no cycle)",
                    ProblemProblemId = Guid.Parse("63060846-b7e7-4584-8b16-099e0cd0ff0c")
                }
            ];

            List<TestCaseS3WrapperObject> testCaseS3Partials =
            [
                new()
                {
                    ProblemId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                    TestCases = [
                        new TestCaseS3Partial
                        {
                           TestCaseId = Guid.Parse("7a2264fa-b7a2-4250-ac4b-a868f746c978"),
                           Expected = "false",
                           Call = ["cycleTest1_node1"],
                           Setup = "${ENTRYPOINT_CLASS_NAME}.Node cycleTest1_node1 = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest1_node2 = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest1_node3 = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest1_node4 = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        cycleTest1_node1.next = cycleTest1_node2;\n        cycleTest1_node2.prev = cycleTest1_node1;\n        cycleTest1_node2.next = cycleTest1_node3;\n        cycleTest1_node3.prev = cycleTest1_node2;\n        cycleTest1_node3.next = cycleTest1_node4;\n        cycleTest1_node4.prev = cycleTest1_node3;"
                        },
                
                        new TestCaseS3Partial
                        {
                           TestCaseId = Guid.Parse("2ed6b7ae-4dd0-4c26-84ee-ce849dd9ce13"),
                           Expected = "true",
                           Call = ["cycleTest2_node1"],
                           Setup = "        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest2_node1 = new ${ENTRYPOINT_CLASS_NAME}.Node(1);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest2_node2 = new ${ENTRYPOINT_CLASS_NAME}.Node(2);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest2_node3 = new ${ENTRYPOINT_CLASS_NAME}.Node(3);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest2_node4 = new ${ENTRYPOINT_CLASS_NAME}.Node(4);\n        cycleTest2_node1.next = cycleTest2_node2;\n        cycleTest2_node2.prev = cycleTest2_node1;\n        cycleTest2_node2.next = cycleTest2_node3;\n        cycleTest2_node3.prev = cycleTest2_node2;\n        cycleTest2_node3.next = cycleTest2_node4;\n        cycleTest2_node4.prev = cycleTest2_node3;\n        cycleTest2_node4.next = cycleTest2_node2;"
                        },
                
                        new TestCaseS3Partial
                        {
                           TestCaseId = Guid.Parse("c2031e76-abf0-4840-8f12-f404df11bb32"),
                           Expected = "true",
                           Call = ["cycleTest3_node1"],
                           Setup = "        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest3_node1 = new ${ENTRYPOINT_CLASS_NAME}.Node(10);\n        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest3_node2 = new ${ENTRYPOINT_CLASS_NAME}.Node(20);\n        cycleTest3_node1.next = cycleTest3_node2;\n        cycleTest3_node2.prev = cycleTest3_node1;\n        cycleTest3_node2.next = cycleTest3_node1; "
                        },
                
                        new TestCaseS3Partial
                        {
                           TestCaseId = Guid.Parse("acb062e7-922f-4ed0-b86c-ac2562a4b959"),
                           Expected = "false",
                           Call = ["cycleTest4_node1"],
                           Setup = "        ${ENTRYPOINT_CLASS_NAME}.Node cycleTest4_node1 = new ${ENTRYPOINT_CLASS_NAME}.Node(5);"
                        },
                    ]
                }
            ];
            
            foreach (var testCaseS3Partial in testCaseS3Partials)
            {
                var objectPath = $"problems/{testCaseS3Partial.ProblemId}/test-cases.xml";
                if (!await s3Client.ObjectExistsAsync(objectPath))
                {
                    await s3Client.PutXmlObjectAsync(objectPath,
                        testCaseS3Partial);
                }
            }

            await context.TestCases.AddRangeAsync(testCases);
            await context.SaveChangesAsync();
        }
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
                new Category { CategoryId = Guid.Parse("e3ccee7d-e1e0-4a61-bb2f-2593e194f8ef"), CategoryName = "test category 1" },
                new Category { CategoryId = Guid.Parse("3ecb6530-4f19-4342-b08e-af746c268a22"), CategoryName = "test category 2" },
                new Category { CategoryId = Guid.Parse("9f23ec20-bf61-4b9f-a509-45e4c9838a3b"), CategoryName = "test category 3" },
                new Category { CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"), CategoryName = "test category 4" },
                new Category { CategoryId = Guid.Parse("4b29315e-e0e1-4aad-8eb6-1bb75eed59f2"), CategoryName = "test category 5" },
                new Category { CategoryId = Guid.Parse("ad4013d2-bdcd-47f7-bc4f-f08942eaa208"), CategoryName = "test category 6" },
                new Category { CategoryId = Guid.Parse("a6fc55c1-a7aa-4532-a734-4cff16b9a4ed"), CategoryName = "test category 7" },
                new Category { CategoryId = Guid.Parse("8efcc3db-c1bb-4284-8c6e-134a94058647"), CategoryName = "test category 8" },
                new Category { CategoryId = Guid.Parse("02178b3a-a68f-4da3-bcb0-5e8deabfc41e"), CategoryName = "test category 9" },
                new Category { CategoryId = Guid.Parse("3dac81de-4bd7-4b58-b091-be911a5db160"), CategoryName = "test category 20" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedDifficulties()
    {
        if (!await context.Difficulties.AnyAsync())
        {
            var difficulties = new List<Difficulty>
            {
                new Difficulty { DifficultyId = Guid.Parse("79f9390e-4b7f-4c1f-a615-b1c6e2caa411"), DifficultyName = "EASY" },
                new Difficulty { DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"), DifficultyName = "MEDIUM" },
                new Difficulty { DifficultyId = Guid.Parse("dc08e91d-c0cd-4dee-80d9-30d7634e0917"), DifficultyName = "HARD" }
            };

            await context.Difficulties.AddRangeAsync(difficulties);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedLanguages()
    {
        if (!await context.Languages.AnyAsync())
        {
            var languages = new List<Language>
            {
                new Language { LanguageId = Guid.Parse("0fd5d3a8-48c1-451b-bcdf-cf414cc6d477"), Name = "java", Version = "17.0.12" }
            };

            await context.Languages.AddRangeAsync(languages);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedItems()
    {
        if (!await context.Items.AnyAsync())
        {
            var items = new List<Item>
            {
                new Item { 
                    ItemId = Guid.Parse("16d4a949-0f5f-481a-b9d6-e0329f9d7dd3"), 
                    Name = "pirate", 
                    Description = "description", 
                    Price = 500, 
                    Purchasable = true, 
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a") 
                },
                new Item { 
                    ItemId = Guid.Parse("052b219a-ec0b-430a-a7db-95c5db35dfce"), 
                    Name = "detective", 
                    Description = "description", 
                    Price = 500, 
                    Purchasable = true, 
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a") 
                },
                new Item { 
                    ItemId = Guid.Parse("03a4dced-f802-4cc5-b239-e0d4c3be9dcd"), 
                    Name = "princess", 
                    Description = "description", 
                    Price = 500, 
                    Purchasable = true, 
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a") 
                }
            };

            await context.Items.AddRangeAsync(items);
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
                    ProblemId = Guid.Parse("63060846-b7e7-4584-8b16-099e0cd0ff0c"),
                    ProblemTitle = "test title 1",
                    Description = "description",
                    CreatedAt = DateTime.UtcNow,
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                    DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
                },
                new Problem
                {
                    ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                    ProblemTitle = "Linked List Cycle Detection",
                    Description = "Implement a method to detect cycles in a linked list using the tortoise and hare algorithm. The solution should include a Node class with next and previous references, and a method that checks for cycles starting from a given node.",
                    CreatedAt = DateTime.UtcNow,
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                    DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
                }
            };

            await context.Problems.AddRangeAsync(problems);
            await context.SaveChangesAsync();
        }
    }
}