namespace InventoryService.Models;

public class AddBookModel
{
    public string BookName { get; set; }
    public string BookAuthor { get; set; }
    public int AvailableCount { get; set; }
}
