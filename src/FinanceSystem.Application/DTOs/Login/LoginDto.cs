using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Login
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
