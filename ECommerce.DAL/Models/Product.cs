using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DAL.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("ProductName")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("ImagePath")]
        public string? ImagePath { get; set; }

        [Required]
        [Column("Price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("MinimumQuantity")]
        public int MinimumQuantity { get; set; }

        [Column("DiscountRate", TypeName = "decimal(5,2)")]
        public decimal DiscountRate { get; set; } = 0;

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("ModifiedDate")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}