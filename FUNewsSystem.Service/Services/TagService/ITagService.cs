using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.TagDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.TagService
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTag();
        Task<Tag?> GetByTagId(Guid id);
        Task<ApiResponseDto<string>> CreateTag(CreateUpdateTagDto dto);
        Task<ApiResponseDto<string>> UpdateTag(int id, CreateUpdateTagDto dto);
        Task<ApiResponseDto<string>> DeleteTag(int id);
    }
}