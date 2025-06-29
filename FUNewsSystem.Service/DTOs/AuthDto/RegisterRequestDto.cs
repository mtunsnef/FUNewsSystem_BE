using System.ComponentModel.DataAnnotations;

namespace FUNewsSystem.Service.DTOs.AuthDto
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public required string ConfirmPassword { get; set; }
    }
}
