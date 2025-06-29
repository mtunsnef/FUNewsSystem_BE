using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto
{
    public class RegisterPayloadDto
    {
        public int ExpiresIn { get; set; } = 3600;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
