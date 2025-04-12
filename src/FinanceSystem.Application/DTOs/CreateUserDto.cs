using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
