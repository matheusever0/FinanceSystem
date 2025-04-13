using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_\.]+$", ErrorMessage = "SystemName deve conter apenas letras, números, underscore e ponto.")]
        public string SystemName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}