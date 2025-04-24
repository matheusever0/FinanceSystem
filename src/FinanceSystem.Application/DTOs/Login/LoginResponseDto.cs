using FinanceSystem.Application.DTOs.User;

namespace FinanceSystem.Application.DTOs.Login
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public required UserDto User { get; set; }
    }
}
