using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto.ExternalDto
{
    public class CreateSystemAccountDto
    {
        public string AccountEmail { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int? AccountRole { get; set; }
        public string? AccountPassword { get; set; }
        public string AuthProvider { get; set; } = null!;
        public string AuthProviderId { get; set; } = null!;
    }

}
