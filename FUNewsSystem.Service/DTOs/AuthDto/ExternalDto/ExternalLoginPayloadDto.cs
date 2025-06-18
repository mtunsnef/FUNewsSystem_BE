using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto.ExternalDto
{
    public class ExternalLoginPayloadDto
    {
        public bool IsNewAccount { get; set; }
        public string? AccessToken { get; set; }
    }

}
