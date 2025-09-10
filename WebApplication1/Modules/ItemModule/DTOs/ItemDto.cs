using WebApplication1.Modules.ItemModule.Models;

namespace WebApplication1.Modules.ItemModule.DTOs;

public class ItemDto(Guid id, string name, string? description, int price, RarityDto rarity)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string? Description { get; init; } = description;
    public int Price { get; init; } = price;
    public RarityDto Rarity { get; init; } = rarity;
}