using InventoryService.Data;
using InventoryService.Enums;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace InventoryService.Controllers;

[ApiController]
[Route("api")]
public class BookController : ControllerBase
{
    private readonly BookService _bookService;

    public BookController(BookService bookService,AppDbContext dbContext)
    {
        _bookService = bookService;
    }

    [HttpPost]
    [Route("add-book")]
    public async Task<IActionResult> AddBook(AddBookModel newBook)
    {

        var addBookTask = Task.Run(() => _bookService.AddBook(newBook));
        
        var completedTask = await Task.WhenAny(addBookTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == addBookTask)
        {
            // The AddBook task completed before the timeout
            var result = await addBookTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
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
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    } 
    
    [HttpGet]
    [Route("get-all-books")]
    public async Task<IActionResult> GetAllBooks()
    {
        var getAllBooksTask =Task.Run(() => _bookService.GetAllBooks());
        var completedTask = await Task.WhenAny(getAllBooksTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getAllBooksTask)
        {
            // The AddBook task completed before the timeout
            var result = await getAllBooksTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
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
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpDelete]
    [Route("remove-book")]
    public async Task<IActionResult> RemoveBook(BaseModel book)
    {
        var removeBookTask =Task.Run(() =>_bookService.RemoveBook(book));
        var completedTask = await Task.WhenAny(removeBookTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == removeBookTask)
        {
            // The AddBook task completed before the timeout
            var result = await removeBookTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }  
    
    [HttpPut]
    [Route("updateInfo/flag={operationType}/{id}")]
    public async Task<IActionResult> UpdateBookInfo(BookEdit operationType, Guid id )
    {
        var updateBookInfoTask =Task.Run(() =>_bookService.UpdateInfo(operationType,id));
        var completedTask = await Task.WhenAny(updateBookInfoTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == updateBookInfoTask)
        {
            // The AddBook task completed before the timeout
            var result = await updateBookInfoTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
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
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }

}