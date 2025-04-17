using FinanceSystem.Application.DTOs.User;

namespace FinanceSystem.Application.DTOs.Login
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; }
    }
}
