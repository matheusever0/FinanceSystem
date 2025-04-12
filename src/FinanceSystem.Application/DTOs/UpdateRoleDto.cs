﻿using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class UpdateRoleDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
