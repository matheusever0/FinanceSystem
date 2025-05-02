using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.Role
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Name { get; set; }

        public required string Description { get; set; }
    }
}
