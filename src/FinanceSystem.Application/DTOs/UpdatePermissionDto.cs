using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class UpdatePermissionDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}