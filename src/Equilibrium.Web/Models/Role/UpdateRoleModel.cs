using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Role
{
    public class UpdateRoleModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [Display(Name = "Descrição")]
        public required string Description { get; set; }
    }
}