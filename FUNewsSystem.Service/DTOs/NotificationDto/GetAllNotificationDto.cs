using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.NotificationDto
{
    public class GetAllNotificationDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string Link { get; set; } = null!;

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Image { get; set; }
    }
}
