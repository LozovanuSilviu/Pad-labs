using InventoryService.Data;
using InventoryService.Data.Entities;
using InventoryService.Enums;
using InventoryService.Models;
using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json;
using RestSharp;

namespace InventoryService.Services;

public class BookService
{
    private readonly AppDbContext _dbContext;
    private List<string> metricsResponse = new List<string>();

    public BookService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
    }
    

    public async Task<List<BookDetail>> SearchBook(string criteria)
    {
        var cl1 = new HttpClient();
        var req =await cl1.GetAsync( "http://identity:80/api/identity/get-libraries");
        var respone =await  req.Content.ReadAsStringAsync();
        var libraries = JsonConvert.DeserializeObject<List<Library>>(respone).AsQueryable();
        try
        {
            var searchCriteria = criteria.ToLower();
            var books =_dbContext.Books.Where(x => x.bookAuthor.ToLower().Contains(searchCriteria) || x.bookName.ToLower().Contains(searchCriteria))
                .Distinct()
                .ToList();
            var booksDetails = new List<BookDetail>();
            
            foreach (var book in books)
            {
                
                var bookDetail = new BookDetail()
                {
                    bookId = book.bookId,
                    bookName = book.bookName,
                    bookAuthor = book.bookAuthor,
                    availableCount = book.availableCount,
                    reservedCount = book.reservedCount,
                    libraryId = book.libraryId,
                    libraryName = libraries.FirstOrDefault(x => x.libraryId.Equals(book.libraryId)).name
                };
                booksDetails.Add(bookDetail);
            }

            return await Task.FromResult(booksDetails);
        }
        catch (Exception e)
        {
            throw new Exception("No books found for this search criteria. Details:" + $"{e.Message}");
        }
   
    }
    
     public async Task<Book> AddBook(AddBookModel newBook)
     {
         var client = new RestClient("http://gateway:3000");
         try
         {
             var book = new Book()
             {
                 bookId = Guid.NewGuid(),
                 bookAuthor = newBook.BookAuthor,
                 bookName = newBook.BookName,
                 availableCount = newBook.AvailableCount,
                 libraryId = newBook.libraryId,
                 reservedCount = 0  
             };
             _dbContext.Add(book);
             _dbContext.SaveChanges();
             var request = new RestRequest("/api/clear-cache", Method.Post);
             var res =await client.ExecuteAsync(request);
             return await Task.FromResult(book);
         }
         catch (Exception e)
         {
             throw new Exception("Wrong data input, Details:"+$"{e.Message}");
         }
       
     }

     public async Task<List<BookDetail>> GetAllBooks()
     {
         var client = new RestClient("http://gateway:3000");
         var cl1 = new HttpClient();
         // try
         // {
             var request = new RestRequest("/api/data?cacheKey=data", Method.Get);
             var response = await client.ExecuteAsync(request);
             Console.WriteLine(response.Content);
             var deserialized = JsonConvert.DeserializeObject<CacheData>(response.Content);
             if (deserialized.Data == null)
             {
                 var req =await cl1.GetAsync( "http://identity:80/api/identity/get-libraries");
                 var respone =await  req.Content.ReadAsStringAsync();
                 var libraries = JsonConvert.DeserializeObject<List<Library>>(respone).AsQueryable();
                 var books = _dbContext.Books.ToList();
                 var booksDetails = new List<BookDetail>();
                 foreach (var book in books)
                 {
                     var bookDetail = new BookDetail()
                     {
                         bookId = book.bookId,
                         bookName = book.bookName,
                         bookAuthor = book.bookAuthor,
                         availableCount = book.availableCount,
                         reservedCount = book.reservedCount,
                         libraryId = book.libraryId,
                         libraryName = libraries.FirstOrDefault(x => x.libraryId.Equals(book.libraryId)).name
                     };
                     booksDetails.Add(bookDetail);
                 }
                 
                 Console.WriteLine("========");
                 var cache = new CacheModel()
                 {
                     cacheKey = "data",
                     data = JsonConvert.SerializeObject(booksDetails)
                 };
                 var saveCacheRequest = new RestRequest("/api/data", Method.Post);
                 saveCacheRequest.AddBody(JsonConvert.SerializeObject(cache));
                 await client.ExecuteAsync(saveCacheRequest);
                 return await Task.FromResult<List<BookDetail>>(booksDetails);
             }
             else
             {
                 return await Task.FromResult<List<BookDetail>>(JsonConvert.DeserializeObject<List<BookDetail>>(deserialized.Data));
             }
           
         // }
         // catch (Exception e)
         // {
         //     throw new Exception("No books found");
         // }
         
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

     public async Task<string> RemoveBook(BaseModel book)
     {
         var client = new RestClient("http://gateway:3000");
         try
         {
             var bookToRemove = _dbContext.Books.FirstOrDefault(x => x.bookId.Equals(book.bookId));
             _dbContext.Remove(bookToRemove);
             _dbContext.SaveChanges();
             var clearCache = new RestRequest("/api/clear-cache", Method.Post);
             await client.ExecuteAsync(clearCache);
             return await Task.FromResult("Successfully removed");
         }
         catch (Exception e)
         {
             return await Task.FromResult("An error occurred while removing the book");
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
                 case BookEdit.LeaseFromReservation:
                 {
                     book.reservedCount -= 1;
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

     public async Task<string> GetMetrics(Dictionary<int,int> requestCounts)
     {
         metricsResponse.Add("# HELP http_requests_total The total number of HTTP requests.");
         metricsResponse.Add("# TYPE http_requests_total counter" );
         foreach (var pair in requestCounts)
         {
             metricsResponse.Add($"http_requests_total{{code=\"{pair.Key}\"}} {pair.Value}" );
         }
         return  String.Join("\n", metricsResponse); 
     }
}