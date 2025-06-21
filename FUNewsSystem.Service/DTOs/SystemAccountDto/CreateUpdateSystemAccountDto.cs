using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.SystemAccountDto
{
    public class CreateUpdateSystemAccountDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public short RoleId { get; set; }
    }
}
