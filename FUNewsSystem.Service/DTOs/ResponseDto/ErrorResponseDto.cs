using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.ResponseDto
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
        public string Message { get; set; } = string.Empty;
        public string? Path { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
