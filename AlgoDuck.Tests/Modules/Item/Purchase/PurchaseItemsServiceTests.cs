using AlgoDuck.Modules.Item.Commands.PurchaseItem;
using Moq;

namespace AlgoDuck.Tests.Modules.Item.Purchase;

public class PurchaseItemServiceTests
{
    private readonly Mock<IPurchaseItemRepository> _mockRepository;
    private readonly PurchaseItemService _service;

    public PurchaseItemServiceTests()
    {
        _mockRepository = new Mock<IPurchaseItemRepository>();
        _service = new PurchaseItemService(_mockRepository.Object);
    }

    [Fact]
    public async Task PurchaseItemAsync_CallsRepository()
    {
        var request = new PurchaseRequestInternalDto
        {
            RequestingUserId = Guid.NewGuid(),
            PurchaseRequestDto = new PurchaseRequestDto { ItemId = Guid.NewGuid() }
        };

        var expectedResult = new PurchaseResultDto { ItemId = request.PurchaseRequestDto.ItemId };
        
        _mockRepository
            .Setup(r => r.PurchaseItemAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _service.PurchaseItemAsync(request, CancellationToken.None);

        Assert.Equal(expectedResult.ItemId, result.ItemId);
        _mockRepository.Verify(
            r => r.PurchaseItemAsync(request, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }
}