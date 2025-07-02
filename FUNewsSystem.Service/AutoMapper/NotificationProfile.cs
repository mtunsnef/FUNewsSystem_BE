using AutoMapper;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.NotificationDto;
using FUNewsSystem.Service.Services.NotificationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.AutoMapper
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile() {
            CreateMap<Notification, GetAllNotificationDto>();
            CreateMap<GetAllNotificationDto, Notification>();

        }
    }
}
