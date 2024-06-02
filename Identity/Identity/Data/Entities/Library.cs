using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Data.Entities;

[Table("Libraries", Schema = "identity")]
public class Library
{
    public Guid libraryId { get; set; }
    public string name { get; set; }
    public string address { get; set; }
}