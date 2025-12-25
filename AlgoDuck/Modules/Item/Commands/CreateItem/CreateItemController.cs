using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Item.Commands.CreateItem;

[ApiController]
[Route("/api/[controller]")]
[Authorize/*("admin")*/]
public class CreateItemController(
    ICreateItemService createItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateItemAsync(
        [FromBody] ItemCreateDto createItemDto,
        CancellationToken cancellation)
    {
     return Ok(new StandardApiResponse<ItemCreateResponseDto>
     {
         Body = await createItemService.CreateItemAsync(createItemDto, cancellation) 
     });   
    }    
}

public interface ICreateItemService
{
    public Task<ItemCreateResponseDto> CreateItemAsync(ItemCreateDto createItemDto, CancellationToken cancellation);
}

public class CreateItemService(
    ICreateItemRepository createItemRepository
    ) : ICreateItemService
{
    public async Task<ItemCreateResponseDto> CreateItemAsync(ItemCreateDto createItemDto, CancellationToken cancellation)
    {
        return await createItemRepository.CreateItemAsync(createItemDto, cancellation);
    }
}

public interface ICreateItemRepository
{
    public Task<ItemCreateResponseDto> CreateItemAsync(ItemCreateDto createItemDto, CancellationToken cancellation);
}

public class CreateItemRepository(
    ApplicationCommandDbContext dbContext, 
    IAwsS3Client s3Client
    ) : ICreateItemRepository
{
    public Task<ItemCreateResponseDto> CreateItemAsync(ItemCreateDto createItemDto, CancellationToken cancellation)
    {
        
        throw new NotImplementedException();
    }
}

public class ItemCreateDto
{
    public required IFormFile File { get; set; }
    public required double ItemCost { get; set; }
}

public class ItemCreateResponseDto
{
    public Guid CreatedItemGuid { get; set; }
}