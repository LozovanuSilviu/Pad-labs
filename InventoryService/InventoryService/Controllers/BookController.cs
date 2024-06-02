using System.Diagnostics.CodeAnalysis;
using InventoryService.Data;
using InventoryService.Enums;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
public class BookController : ControllerBase
{
    private readonly BookService _bookService;
    private static readonly Dictionary<int, int> RequestCounts = new Dictionary<int, int>() {{200,0},{400,0},{500,0},{404,0}};

    public BookController(BookService bookService,AppDbContext dbContext)
    {
        _bookService = bookService;
    }

    [HttpPost]
    [Route("add-book")]
    public async Task<IActionResult> AddBook(AddBookModel newBook)
    {
        try
        {
            var addBookTask = Task.Run(() => _bookService.AddBook(newBook));
        
            var completedTask = await Task.WhenAny(addBookTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
            if (completedTask == addBookTask)
            {
                // The AddBook task completed before the timeout
                var result = await addBookTask;
                RequestCounts[200]++;
                return Ok(result);
            }
            else
            {
                // The timeout task completed before the AddBook task
                RequestCounts[500]++;
                return StatusCode(500, "Operation timed out.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            RequestCounts[500]++;
            return StatusCode(500,$"{e.Message}");
        }
       
    } 
    
    [HttpGet]
    [Route("search/{criteria}")]
    public async Task<IActionResult> SearchBook(string criteria)
    {
        var searchBookTask =Task.Run(() =>_bookService.SearchBook(criteria));
        var completedTask = await Task.WhenAny(searchBookTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == searchBookTask)
        {
            // The AddBook task completed before the timeout
            var result = await searchBookTask;
            RequestCounts[200]++;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            RequestCounts[500]++;
            return StatusCode(500, "Operation timed out.");
        }
    } 
    
    [HttpGet]
    [Route("get-all-books")]
    public async Task<IActionResult> GetAllBooks()
    {
        return Ok(await _bookService.GetAllBooks() ) ;
    }
    
    [HttpGet]
    [Route("get-book-by-id/{id}")]
    public async Task<IActionResult> GetBookById(Guid id)
    {
        var getBookByIdTask =Task.Run(() => _bookService.GetBookById(id));
        var completedTask = await Task.WhenAny(getBookByIdTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getBookByIdTask)
        {
            // The AddBook task completed before the timeout
            var result = await getBookByIdTask;
            RequestCounts[200]++;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            RequestCounts[500]++;
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpDelete]
    [Route("remove-book")]
    public async Task<IActionResult> RemoveBook(BaseModel book)
    {
        await _bookService.RemoveBook(book);
        return Ok(200);
    }  
    
    [HttpPut]
    [Route("updateInfo/flag={operationType}/{id}")]
    public async Task<IActionResult> UpdateBookInfo(BookEdit operationType, Guid id )
    {
        var updateBookInfoTask =Task.Run(() =>_bookService.UpdateInfo(operationType,id));
        var completedTask = await Task.WhenAny(updateBookInfoTask, Task.Delay(TimeSpan.FromMilliseconds(4000)));
        
        if (completedTask == updateBookInfoTask)
        {
            // The AddBook task completed before the timeout
            var result = await updateBookInfoTask;
            RequestCounts[200]++;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            RequestCounts[500]++;
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpGet]
    [Route("health")]
    public async Task<IActionResult> GetHealthStatus()
    {
        var getHealthStatusTask =Task.Run(() =>_bookService.GetHealthStatus(_bookService));
        var completedTask = await Task.WhenAny(getHealthStatusTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getHealthStatusTask)
        {
            // The AddBook task completed before the timeout
            var result = await getHealthStatusTask;
            RequestCounts[200]++;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            RequestCounts[500]++;
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpGet]
    [Route("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var getMetricsTask =Task.Run(() =>_bookService.GetMetrics(RequestCounts));
        var completedTask = await Task.WhenAny(getMetricsTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getMetricsTask)
        {
            // The AddBook task completed before the timeout
            var result = await getMetricsTask;
            return Ok(result);
        }   
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }

}