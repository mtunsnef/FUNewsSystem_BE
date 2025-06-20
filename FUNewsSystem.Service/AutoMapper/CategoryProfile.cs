using AutoMapper;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.CategoryDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.AutoMapper
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CreateUpdateCategoryDto, Category>();
            CreateMap<Category, CreateUpdateCategoryDto>();
            CreateMap<Category, GetAllCategoriesDto>();
        }
    }
}
