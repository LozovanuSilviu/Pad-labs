using System.ComponentModel.DataAnnotations.Schema;
using Identity.Enums;

namespace Identity.Data.Entities;

[Table("Users", Schema = "identity")]
public class User
{
    public Guid userId { get; set; }
    public string userName { get; set; }
    public long idnp { get; set; }
    public string hashedPassword { get; set; }
    public UserType userType { get; set; }
    public string email { get; set; }
    public Guid libraryId { get; set; }
}