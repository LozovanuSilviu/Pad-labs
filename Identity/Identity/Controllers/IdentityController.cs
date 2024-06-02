using System.Security.Cryptography.X509Certificates;
using Identity.Enums;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController : ControllerBase
{
    private readonly IdentityService _identityService;
    public IdentityController(IdentityService identityService)
    {
        _identityService = identityService;
    }
    [HttpPost]
    [Route("login")]
    public  IActionResult Login(LoginModel login)
    {
        try
        {
            return Ok( _identityService.Login(login));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost]
    [Route("signUp")]
    public async Task<IActionResult> SignUp(SignUpModel signup,
    [FromQuery] UserType userType = UserType.Reader,
    [FromQuery] Guid libraryId = default
        )
    {
        try
        {
            return Ok(await _identityService.Signup(signup,userType,libraryId));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }  
    
    [HttpPost]
    [Route("add-library")]
    public async Task<IActionResult> AddLibrary(NewLibraryModel library)
    {
      
            return Ok(await _identityService.AddLibrary(library));
    }

    [HttpGet]
    [Route("get-libraries")]
    public async Task<IActionResult> GetLibraries()
    {
        return Ok(await _identityService.GetLibraries());
    } 
    
    [HttpGet]
    [Route("get-users")]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await _identityService.GetUsers());
    } 
    [HttpGet]
    [Route("get-user")]
    public async Task<IActionResult> GetUser(
        [FromQuery] string userId)
    {
        return Ok(await _identityService.GetUser(userId));
    }
}