using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCityBackend.Models;

[Table("role")]
public class Role : AuditableEntity
{
    public string Name { get; set; } = null!;

    public IEnumerable<User> Users { get; set; } = new List<User>();
}