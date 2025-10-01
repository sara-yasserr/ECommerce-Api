using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BLL.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product code is required")]
        [StringLength(20, ErrorMessage = "Product code cannot exceed 20 characters")]
        [RegularExpression(@"^P\d{3}$", ErrorMessage = "Product code must be in format P001, P002, etc.")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Minimum quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity must be at least 1")]
        public int MinimumQuantity { get; set; }

        [Range(0, 100, ErrorMessage = "Discount rate must be between 0 and 100")]
        public decimal DiscountRate { get; set; } = 0;
    }
}
