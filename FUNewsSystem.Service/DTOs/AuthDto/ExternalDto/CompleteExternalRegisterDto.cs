using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.AuthDto.ExternalDto
{
    public class CompleteExternalRegisterDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string ExternalId { get; set; }
        public string Provider { get; set; }
    }
}
