using InventoryService.Data.Entities;

namespace InventoryService.Models;

public class CacheData
{
        public string Source { get; set; }
        public List<Book> Data { get; set; }
        public string Message { get; set; }
}