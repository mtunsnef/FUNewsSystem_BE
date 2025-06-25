using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto.TwoFaDto
{
    public class TwoFaInitDto
    {
        public string SharedKey { get; set; }
        public string QrCodeUri { get; set; }
    }

}
