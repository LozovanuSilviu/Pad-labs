using Identity.Enums;

namespace Identity.Models;

public class LoginResponse
{
    public UserType userType { get; set; }
    public string userName { get; set; }
    public Guid userId { get; set; }
    public Guid? libraryId { get; set; }
}