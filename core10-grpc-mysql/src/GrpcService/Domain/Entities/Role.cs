using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace core10_grpc.Entities;

[Table("roles")]
public class Role {

    [Key]
    public int Id {get; set;}

    [Column("name",TypeName="varchar(20)")]
    public string Name {get; set;}

    public ICollection<User> Users { get; set; } = new List<User>();
}
    
