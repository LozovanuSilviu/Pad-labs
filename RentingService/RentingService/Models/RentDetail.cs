namespace RentingService.Models;

public class RentDetail
{
    public Guid leaseId { get; set; }
    public string leaseStartDate { get; set; }
    public string returnDate { get; set; }
    public Guid bookId { get; set; }
    public string customerName { get; set; }
}