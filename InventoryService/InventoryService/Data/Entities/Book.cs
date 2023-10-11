using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Data.Entities;

[Table("Books", Schema = "inventory")]
public class Book
{
    public Guid bookId { get; set; }
    public string bookName { get; set; }
    public string bookAuthor { get; set; }
    public int availableCount { get; set; }
    public int reservedCount { get; set; }
}