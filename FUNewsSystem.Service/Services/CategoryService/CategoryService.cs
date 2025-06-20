using AutoMapper;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.CategoryRepo;
using FUNewsSystem.Service.DTOs.CategoryDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<string>> CreateCategory(CreateUpdateCategoryDto dto)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            bool exists = allCategories.Any(c => c.CategoryName == dto.CategoryName);

            if (exists)
                throw new ConflictException("Category with the same name already exists.");

            var category = _mapper.Map<Category>(dto);
            await _categoryRepository.AddAsync(category);

            return ApiResponseDto<string>.SuccessResponse("Category created successfully.");
        }

        public async Task<ApiResponseDto<string>> UpdateCategory(short id, CreateUpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found.");

            _mapper.Map(dto, category);
            await _categoryRepository.UpdateAsync(category);

            return ApiResponseDto<string>.SuccessResponse("Category updated successfully.");
        }

        public async Task<ApiResponseDto<string>> DeleteCategory(short id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found.");

            await _categoryRepository.DeleteAsync(category);
            return ApiResponseDto<string>.SuccessResponse("Category deleted successfully.");
        }

        public async Task<Category> GetByCategoryIdAsync(short id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found.");

            return category;
        }

        public async Task<List<GetAllCategoriesDto>> GetAllCategoryAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var result = _mapper.Map<List<GetAllCategoriesDto>>(categories);

            return result;
        }
    }
}
