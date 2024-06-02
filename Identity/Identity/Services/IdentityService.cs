using CSharpVitamins;
using Identity.Data;
using Identity.Data.Entities;
using Identity.Enums;
using Identity.Helpers;
using Identity.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace Identity.Services;

public class IdentityService
{
    private  readonly AppDbContext _dbContext;
    
    public IdentityService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public   LoginResponse Login(LoginModel loginModel)
    {
        var user =  _dbContext.Users.FirstOrDefault(x => x.email.Equals(loginModel.username));
        if (user is null) 
        {
            throw new Exception("Such user doesn't exist");
        }
        
        var isPasswordOk = PasswordComparer.ComparePasswordHash(loginModel.password, user!.hashedPassword);
        var response = new LoginResponse()
        {
            userId = user.userId,
            userName = user.userName,
            userType = user.userType
        };
        if (user.userType.Equals(UserType.Librarian))
        {
            response.libraryId = user.libraryId;
        }

        return response;
    }

    public async Task<SignUpResponse> Signup(SignUpModel signup, UserType userType, Guid libraryId)
    {
        try
        {
            var users = _dbContext.Users;
            var validUser = !users.Any(x => x.idnp.Equals(signup.Idnp) ||  x.email.Equals(signup.email));
            if (!validUser)
            {
                throw new Exception("User with such email or Idnp already exists");
            }
            var hashPassword = HashEncoder.HashPassword(signup.password);
            var user = new User()
            {
                userId = ShortGuid.NewGuid(),
                hashedPassword = hashPassword,
                idnp = signup.Idnp,
                userName = signup.userName,
                userType = userType,
                email = signup.email,
                libraryId = libraryId
            };
            _dbContext.Users.Add(user);
           await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {

            throw new Exception($"Failed to save data reason:{e.Message}");
        }

        return new SignUpResponse()
        {
            Message = "Succesfully created user"
        };
    }

    public async Task<AddLibraryResponse> AddLibrary(NewLibraryModel library)
    {
        try
        {
            var newLibrary = new Library()
            {
                libraryId = Guid.NewGuid(),
                name = library.name,
                address = library.address
            };

            _dbContext.Libraries.Add(newLibrary);
            await _dbContext.SaveChangesAsync();
            var response = new AddLibraryResponse()
            {
                message = "Successfully added new library"
            };
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<object?> GetLibraries()
    {
        return await _dbContext.Libraries.ToListAsync();
    }

    public async Task<object?> GetUsers()
    {
        var client = new RestClient("http://renting-first:80");
        var request = new RestRequest("/all-reservations");
        var response = await client.ExecuteAsync(request);
        var deserialized =  JsonConvert.DeserializeObject<List<Reservation>>(response.Content);
        var request2 = new RestRequest("/all-leases");
        var response2 = await client.ExecuteAsync(request2);
        var deserialized2 =  JsonConvert.DeserializeObject<List<Rent>>(response2.Content);
        var users= await _dbContext.Users.Where(x => x.userType != UserType.Librarian).ToListAsync();
        var result = new List<UserDetails>();
        foreach (var user in users)
        {
            var reservationCount = deserialized.Where(x => x.customerName.Equals(user.userName)).Count();
            var leaseCount = deserialized2.Where(x => x.customerName.Equals(user.userName)).Count();
            var userDetail = new UserDetails()
            {
                customerId = user.userId.ToString(),
                customerName = user.userName,
                email = user.email,
                numberOfRentings = leaseCount,
                numberOfReservations = reservationCount
            };
            result.Add(userDetail);
        }

        return result;
    }

    public async Task<User?> GetUser(string userId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.userId.Equals(Guid.Parse(userId)));
    }
}