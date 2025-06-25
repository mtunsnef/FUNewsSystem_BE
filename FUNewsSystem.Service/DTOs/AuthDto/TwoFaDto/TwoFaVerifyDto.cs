using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto.TwoFaDto
{
    public class TwoFaVerifyDto
    {
        public string Code { get; set; }
        public string SharedKey { get; set; }
    }
}
