using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.NotificationDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.NotificationService
{
    public interface INotificationService
    {
        Task<List<GetAllNotificationDto>> GetAllNotificationAsync();
        Task<List<GetAllNotificationDto>> GetNotificationsByUserIdAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task NotifyUserAsync(string userId, string message, string link, string image, DateTime timestamp);
    }
}
