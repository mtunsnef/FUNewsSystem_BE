using AutoMapper;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.TagDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.AutoMapper
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<CreateUpdateTagDto, Tag>();
            CreateMap<Tag, CreateUpdateTagDto>();
        }
    }
}
