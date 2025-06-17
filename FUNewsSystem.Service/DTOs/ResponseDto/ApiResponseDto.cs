using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.ResponseDto
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }

        public ApiResponseDto(bool success, T data, int statusCode = (int)HttpStatusCode.OK)
        {
            Success = success;
            Data = data;
            StatusCode = statusCode;
        }

        public static ApiResponseDto<T> SuccessResponse(T data)
        {
            return new ApiResponseDto<T>(true, data);
        }

        public static ApiResponseDto<T> FailResponse(int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new ApiResponseDto<T>(false, default, statusCode);
        }
    }
}
