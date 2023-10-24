using InventoryService.Data;
using InventoryService.Data.Entities;
using InventoryService.Enums;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api")]
public class BookController
{
    private readonly BookService _bookService;

    public BookController(BookService bookService,AppDbContext dbContext)
    {
        _bookService = bookService;
    }

    [HttpPost]
    [Route("add-book")]
    public async Task<string> AddBook(AddBookModel newBook)
    { 
        await _bookService.AddBook(newBook);
        return "Succesfully added";
    } 
    
    [HttpGet]
    [Route("search/{criteria}")]
    public async Task<List<Book>> SearchBook(string criteria)
    {
        var response =await _bookService.SearchBook(criteria);
        return response;
    } 
    
    [HttpGet]
    [Route("get-all-books")]
    public async Task<List<Book>> GetAllBooks()
    {
        var response =await _bookService.GetAllBooks();
        return response;
    }
    
    [HttpDelete]
    [Route("remove-book")]
    public async Task<string> RemoveBook(BaseModel book)
    {
        var response =await _bookService.RemoveBook(book);
        return response;
    }  
    
    [HttpPut]
    [Route("updateInfo/flag={operationType}/{id}")]
    public async Task<Book> UpdateBookInfo(BookEdit operationType, Guid id )
    {
        var response =await _bookService.UpdateInfo(operationType,id);
        return response;
    }
    
    [HttpGet]
    [Route("updateInfo/flag={operationType}/{id}")]
    public async Task<HealthStatus> GetHealthStatus(BookEdit operationType, Guid id )
    {
        var response =await _bookService.GetHealthStatus(_bookService);
        return response;
    }

}