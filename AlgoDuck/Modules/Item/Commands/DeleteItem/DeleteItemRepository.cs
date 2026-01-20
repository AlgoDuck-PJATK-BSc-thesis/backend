using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Commands.DeleteItem;

public interface IDeleteItemRepository
{
    public Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class DeleteItemRepository : IDeleteItemRepository
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IAwsS3Client _awsS3Client;

    public DeleteItemRepository(ApplicationCommandDbContext dbContext, IAwsS3Client awsS3Client)
    {
        _dbContext = dbContext;
        _awsS3Client = awsS3Client;
    }

    public async Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var itemType = await _dbContext.Items
            .Where(i => i.ItemId == itemId)
            .Select(i => EF.Property<string>(i, "type"))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (itemType == null)
            return Result<DeleteItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not delete item: {itemId}"));
        
        var rowsChanged = await _dbContext.Items.Where(i => i.ItemId == itemId).ExecuteDeleteAsync(cancellationToken: cancellationToken);
        
        if (rowsChanged == 0) 
            return Result<DeleteItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"Could not delete item: {itemId}"));

        var itemContentPrefix = $"{itemType}s/{itemId}";
        await _awsS3Client.DeleteAllByPrefixAsync(itemContentPrefix, S3BucketType.Content, cancellationToken);
        
        return Result<DeleteItemResultDto, ErrorObject<string>>.Ok(new DeleteItemResultDto
        {
            ItemId = itemId
        });
    }
}