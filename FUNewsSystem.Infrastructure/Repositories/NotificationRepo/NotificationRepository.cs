using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.DataAccess;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.NotificationRepo
{
    public class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
    {

        public NotificationRepository(FunewsSystemApiDbContext context)
            : base(context)
        {
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10).ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;

            await UpdateAsync(notification);
            return true;
        }
    }
}
