using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Role
{
    public class UpdateRoleModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}