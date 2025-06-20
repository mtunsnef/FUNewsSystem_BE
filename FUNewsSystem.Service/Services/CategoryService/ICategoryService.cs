using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.CategoryDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Category> GetByCategoryIdAsync(short id);
        Task<ApiResponseDto<string>> CreateCategory(CreateUpdateCategoryDto dto);
        Task<ApiResponseDto<string>> UpdateCategory(short id, CreateUpdateCategoryDto dto);
        Task<ApiResponseDto<string>> DeleteCategory(short id);
        Task<List<GetAllCategoriesDto>> GetAllCategoryAsync();
    }
}
