using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } 

        public List<string> Roles { get; set; }

        public bool? IsActive { get; set; }
    }
}