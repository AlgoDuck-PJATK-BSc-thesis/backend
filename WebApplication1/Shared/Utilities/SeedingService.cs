using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Modules.ItemModule.Models;
using WebApplication1.Modules.ProblemModule.Models;

namespace WebApplication1.Shared.Utilities;

public class DataSeedingService(ApplicationDbContext context)
{
    public async Task SeedDataAsync()
    {
        await SeedRarities();
        await SeedCategories();
        await SeedDifficulties();
        await SeedProblemTypes();
        await SeedLanguages();
        await SeedItems();
        await SeedProblems();
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

    private async Task SeedProblemTypes()
    {
        if (!await context.ProblemTypes.AnyAsync())
        {
            var problemTypes = new List<ProblemType>
            {
                new ProblemType { ProblemTypeId = Guid.Parse("e10b7916-04ad-48fd-a099-0c6845b299a4"), Name = "test1" },
                new ProblemType { ProblemTypeId = Guid.Parse("71c8cba5-0a26-41ae-af6f-b6f335b2a1a7"), Name = "test2" },
                new ProblemType { ProblemTypeId = Guid.Parse("9c0a7410-2efb-4302-94fe-134f6c034e5e"), Name = "test3" },
                new ProblemType { ProblemTypeId = Guid.Parse("8f58d296-1562-4ef1-89b4-dcc05652e9d8"), Name = "test4" },
                new ProblemType { ProblemTypeId = Guid.Parse("604d6d1b-6b66-49e0-94ac-b72dbdb28be1"), Name = "test5" }
            };

            await context.ProblemTypes.AddRangeAsync(problemTypes);
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
                    Picture = "...", 
                    Description = "description", 
                    Price = 500, 
                    Purchasable = true, 
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a") 
                },
                new Item { 
                    ItemId = Guid.Parse("052b219a-ec0b-430a-a7db-95c5db35dfce"), 
                    Name = "detective", 
                    Picture = "...", 
                    Description = "description", 
                    Price = 500, 
                    Purchasable = true, 
                    RarityId = Guid.Parse("072ed5ba-929c-4b67-adb6-c747a3a1404a") 
                },
                new Item { 
                    ItemId = Guid.Parse("03a4dced-f802-4cc5-b239-e0d4c3be9dcd"), 
                    Name = "princess", 
                    Picture = "...", 
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
                    CreatedAt = DateTime.SpecifyKind(DateTime.Parse("2025-08-16 20:36:00.239201"), DateTimeKind.Utc),
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                    DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
                    ProblemTypeId = Guid.Parse("71c8cba5-0a26-41ae-af6f-b6f335b2a1a7"),
                    DuelId = null
                },
                new Problem
                {
                    ProblemId = Guid.Parse("3152daea-43cd-426b-be3b-a7e6d0e376e1"),
                    ProblemTitle = "Linked List Cycle Detection",
                    Description = "Implement a method to detect cycles in a linked list using the tortoise and hare algorithm. The solution should include a Node class with next and previous references, and a method that checks for cycles starting from a given node.",
                    CreatedAt = DateTime.SpecifyKind(DateTime.Parse("2025-08-18 16:59:51.370235"), DateTimeKind.Utc),
                    CategoryId = Guid.Parse("d018bd6e-2cb0-412c-939f-27b3cf654e58"),
                    DifficultyId = Guid.Parse("07c41ca9-9077-471a-ae30-3ff8f0b40c9a"),
                    ProblemTypeId = Guid.Parse("71c8cba5-0a26-41ae-af6f-b6f335b2a1a7"),
                    DuelId = null
                }
            };

            await context.Problems.AddRangeAsync(problems);
            await context.SaveChangesAsync();
        }
    }
}