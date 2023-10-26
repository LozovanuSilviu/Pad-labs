using InventoryService.Data;
using InventoryService.Data.Entities;
using InventoryService.Enums;
using InventoryService.Models;
using Newtonsoft.Json;
using RestSharp;

namespace InventoryService.Services;

public class BookService
{
    private readonly AppDbContext _dbContext;
    private readonly RestClient client;
    public BookService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
         client = new RestClient("http://cache:1234");
    }

    public Task<List<Book>> SearchBook(string criteria)
    {
        try
        {
            var searchCriteria = criteria.ToLower();
            var books =_dbContext.Books.Where(x => x.bookAuthor.ToLower().Contains(searchCriteria) || x.bookName.ToLower().Contains(searchCriteria))
                .Distinct()
                .ToList();
            return Task.FromResult(books);
        }
        catch (Exception e)
        {
            throw new Exception("No books found for this search criteria. Details:" + $"{e.Message}");
        }
   
    }
    
     public async Task<string> AddBook(AddBookModel newBook)
     {
         try
         {
             var book = new Book()
             {
                 bookId = Guid.NewGuid(),
                 bookAuthor = newBook.BookAuthor,
                 bookName = newBook.BookName,
                 availableCount = newBook.AvailableCount,
                 reservedCount = 0
             };
             _dbContext.Add(book);
             _dbContext.SaveChanges();
             var request = new RestRequest("/api/data", Method.Post);
             var dataCache = await GetAllBooks();
             var cache = new CacheModel()
             {
                 cacheKey = "data",
                 data = JsonConvert.SerializeObject(dataCache)
             };
             request.AddBody(JsonConvert.SerializeObject(cache));
             var res =await client.ExecuteAsync(request);
             return await Task.FromResult("Successfully added"+res.Content);
         }
         catch (Exception e)
         {
             throw new Exception("Wrong data input, Details:"+$"{e.Message}");
         }
       
     }

     public Task<List<Book>> GetAllBooks()
     {
         try
         {
             var books = _dbContext.Books.ToList();
             return Task.FromResult<List<Book>>(books);
         }
         catch (Exception e)
         {
             throw new Exception("No books found");
         }
         
     }
     public Task<Book> GetBookById(Guid id)
     {
         try
         {
             var book = _dbContext.Books.FirstOrDefault(x => x.bookId.Equals(id));
             return Task.FromResult<Book>(book);
         }
            catch (Exception e)
         {
             throw new Exception("No books found");
         }
         
     }

     public  Task<string> RemoveBook(BaseModel book)
     {
         try
         {
             var bookToRemove = _dbContext.Books.FirstOrDefault(x => x.bookId.Equals(book.bookId));
             _dbContext.Remove(bookToRemove);
             _dbContext.SaveChanges();
             return Task.FromResult("Successfully removed");
         }
         catch (Exception e)
         {
             return Task.FromResult("An error occurred while removing the book");
         }
     }

     public  Task<Book?> UpdateInfo(BookEdit operationType, Guid id)
     {
         try
         {
             var book = _dbContext.Books.FirstOrDefault(x => x.bookId.Equals(id));
             switch (operationType)
             {
                 case BookEdit.Reserve:
                 {
                     book.availableCount-=1;
                     book.reservedCount+=1;
                     break;
                 } 
                 case BookEdit.CancelReservation:
                 {
                     book.availableCount+=1;
                     book.reservedCount-=1;
                     break;
                 }
                 case BookEdit.Lease:
                 {
                     book.availableCount-=1;
                     break;
                 } case BookEdit.Return:
                 {
                     book.availableCount+=1;
                     break;
                 }
             }
             _dbContext.SaveChanges();
             return Task.FromResult(book);
         }
         catch (Exception e)
         {
             throw new Exception("Something went wrong. Details:" + $"{e.Message}");
         }
     }
     
     public async Task<HealthStatus> GetHealthStatus(BookService service)
     {
         var connected = _dbContext.Database.CanConnect();
         bool serviceInitialized = !service.Equals(null);
         var status = new HealthStatus()
         {
             database = "disconnected",
             ready = false
         };
         if (connected && serviceInitialized)
         {
             status.database = "connected";
             status.ready = true;
         }
         else if (connected && !serviceInitialized)
         {
             status.ready = false;
         }

         return status;
     }
}