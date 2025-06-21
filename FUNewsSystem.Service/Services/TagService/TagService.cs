using AutoMapper;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.TagRepo;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.TagDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public TagService(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }


        public Task<Tag?> GetByTagId(Guid id)
        {
            var tag = _tagRepository.GetByIdAsync(id);
            if (tag == null)
                throw new NotFoundException($"Tag with Id {id} not found.");

            return tag;
        }

        public async Task<ApiResponseDto<string>> CreateTag(CreateUpdateTagDto dto)
        {
            var allTags = await _tagRepository.GetAllAsync();
            bool exists = allTags.Any(c => c.TagName == dto.TagName);

            if (exists)
                throw new ConflictException("Tag with the same name already exists.");

            var tag = _mapper.Map<Tag>(dto);
            await _tagRepository.AddAsync(tag);

            return ApiResponseDto<string>.SuccessResponse("Tag created successfully.");
        }

        public async Task<ApiResponseDto<string>> UpdateTag(int id, CreateUpdateTagDto dto)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                throw new NotFoundException($"Tag with Id {id} not found.");

            _mapper.Map(dto, tag);
            await _tagRepository.UpdateAsync(tag);

            return ApiResponseDto<string>.SuccessResponse("Tag updated successfully.");
        }

        public async Task<ApiResponseDto<string>> DeleteTag(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                throw new NotFoundException($"Tag with Id {id} not found.");

            await _tagRepository.DeleteAsync(tag);

            return ApiResponseDto<string>.SuccessResponse("Tag deleted successfully.");
        }

        public Task<List<Tag>> GetAllTag()
        {
            return _tagRepository.GetAllAsync();
        }
    }
}
