namespace RentingService.Models;

public class BookModel
{
    public Guid bookId { get; set; }
    public string bookName { get; set; }
    public string bookAuthor { get; set; }
    public int availableCount { get; set; }
    public int reservedCount { get; set; }
}