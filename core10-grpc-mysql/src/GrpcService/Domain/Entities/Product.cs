using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace core10_grpc.Entities;

    
[Table("products")]
public class Product {

        [Key]
        public int Id { get; set; }

        [Column("category",TypeName="varchar(50)")]
        public string Category { get; set; }

        [Column("descriptions",TypeName="varchar(100)")]
        public string Descriptions { get; set; }

        [Column("qty")]
        public int? Qty { get; set; }

        [Column("unit",TypeName="varchar(10)")]
        public string Unit { get; set; }

        [Column("costprice",TypeName="decimal(10,2)")]
        public decimal CostPrice { get; set; }

        [Column("sellprice",TypeName="decimal(10,2)")]
        public decimal SellPrice { get; set; }

        [Column("saleprice",TypeName="decimal(10,2)")]
        public decimal SalePrice { get; set; }

        [Column("productpicture",TypeName="varchar(100)")]
        public string ProductPicture { get; set; }

        [Column("alertstocks")]
        public int AlertStocks { get; set; }

        [Column("criticalstocks")]
        public int CriticalStocks { get; set; }

        [Column("createdAt",TypeName="timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [DefaultValue("getutcdate()")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt",TypeName="timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [DefaultValue("getutcdate()")]
        public DateTime UpdatedAt { get; set; }
}    