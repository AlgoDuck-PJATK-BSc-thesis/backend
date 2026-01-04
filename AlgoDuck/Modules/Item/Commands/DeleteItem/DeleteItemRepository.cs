using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Commands.DeleteItem;

public interface IDeleteItemRepository
{
    public Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class DeleteItemRepository(
    ApplicationCommandDbContext dbContext,
    IAwsS3Client awsS3Client
    ) : IDeleteItemRepository
{
    public async Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var itemType = await dbContext.Items
            .Where(i => i.ItemId == itemId)
            .Select(i => EF.Property<string>(i, "type"))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (itemType == null)
            return Result<DeleteItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not delete item: {itemId}"));
        
        var rowsChanged = await dbContext.Items.Where(i => i.ItemId == itemId).ExecuteDeleteAsync(cancellationToken: cancellationToken);
        
        if (rowsChanged == 0) 
            return Result<DeleteItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"Could not delete item: {itemId}"));

        var itemContentPrefix = $"{itemType}s/{itemId}";
        await awsS3Client.DeleteAllByPrefixAsync(itemContentPrefix, S3BucketType.Content, cancellationToken);
        
        return Result<DeleteItemResultDto, ErrorObject<string>>.Ok(new DeleteItemResultDto
        {
            ItemId = itemId
        });
    }
}