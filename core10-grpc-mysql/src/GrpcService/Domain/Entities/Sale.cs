using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace core10_grpc.Entities;

[Table("sales")]
public class Sale 
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Precision(18, 2)]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("date", TypeName = "datetime")]
    public DateTime Date { get; set; }
}