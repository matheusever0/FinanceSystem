using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.Permission
{
    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_\.]+$", ErrorMessage = "SystemName deve conter apenas letras, números, underscore e ponto.")]
        public required string SystemName { get; set; }

        [StringLength(255)]
        public required string Description { get; set; }
    }
}