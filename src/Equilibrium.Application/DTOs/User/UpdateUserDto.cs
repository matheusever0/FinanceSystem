using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.User
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        public required List<string> Roles { get; set; }

        public bool? IsActive { get; set; }
    }
}