namespace AlgoDuck.Models;

public class PurchasedTestCase
{
    public TestCase? TestCase { get; set; }
    public Guid TestCaseId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid UserId { get; set; }
    
}