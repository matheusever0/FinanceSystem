using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceSystem.Application.DTOs
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
