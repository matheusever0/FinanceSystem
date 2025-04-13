﻿using System.ComponentModel.DataAnnotations;

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
