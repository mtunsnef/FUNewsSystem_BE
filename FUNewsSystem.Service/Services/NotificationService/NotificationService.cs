using AutoMapper;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Messaging;
using FUNewsSystem.Infrastructure.Repositories.NotificationRepo;
using FUNewsSystem.Service.DTOs.NotificationDto;
using Microsoft.AspNetCore.SignalR;

namespace FUNewsSystem.Service.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationPublisher> _hubContext;
        private readonly IMapper _mapper;
        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationPublisher> hubContext, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<List<GetAllNotificationDto>> GetAllNotificationAsync()
        {
            var notifications = await _notificationRepository.GetAllAsync();
            var result = _mapper.Map<List<GetAllNotificationDto>>(notifications);
            return result;
        }


        public async Task<List<GetAllNotificationDto>> GetNotificationsByUserIdAsync(string userId)
        {
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
            var result = _mapper.Map<List<GetAllNotificationDto>>(notifications);
            return result;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task NotifyUserAsync(string userId, string message, string link, string image, DateTime timestamp)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                Image = image,
                CreatedAt = timestamp,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);

            Console.WriteLine($"[SignalR] Gửi notify đến userId = {userId}");

            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", notification);

            Console.WriteLine("[SignalR] Đã gửi xong!");
        }

    }
}
