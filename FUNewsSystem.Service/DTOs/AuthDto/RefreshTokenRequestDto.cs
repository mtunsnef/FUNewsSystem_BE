using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto
{
    public class RefreshTokenRequestDto
    {
        public required string AccessToken { get; set; }
        //public required string RefreshToken { get; set; }
    }
}
