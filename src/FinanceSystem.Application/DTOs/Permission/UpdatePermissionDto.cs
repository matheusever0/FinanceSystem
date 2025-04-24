using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Permission
{
    public class UpdatePermissionDto
    {
        [StringLength(100, MinimumLength = 3)]
        public required string Name { get; set; }

        [StringLength(255)]
        public required string Description { get; set; }
    }
}