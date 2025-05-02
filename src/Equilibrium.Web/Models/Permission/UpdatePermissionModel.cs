using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Permission
{
    public class UpdatePermissionModel
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres")]
        [Display(Name = "Descrição")]
        public required string Description { get; set; }
    }
}