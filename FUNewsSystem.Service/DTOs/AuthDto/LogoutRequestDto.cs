using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto
{
    public class LogoutRequestDto
    {
        public string Token { get; set; } = default!;
    }
}
