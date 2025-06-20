using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.CategoryDto
{
    public class GetAllCategoriesDto
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
