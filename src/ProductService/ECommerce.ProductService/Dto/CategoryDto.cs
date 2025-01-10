using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.ProductService.Dto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }
    }
}