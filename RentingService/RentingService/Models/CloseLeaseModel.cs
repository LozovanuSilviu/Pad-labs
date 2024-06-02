namespace RentingService.Models;

public class CloseLeaseModel
{
    public Guid leaseId { get; set; }
    public string bookId { get; set; }
}