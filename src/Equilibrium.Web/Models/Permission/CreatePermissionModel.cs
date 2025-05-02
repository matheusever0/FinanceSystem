using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Permission
{
    public class CreatePermissionModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "O nome do sistema é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome do sistema deve ter entre 3 e 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_\.]+$", ErrorMessage = "O nome do sistema deve conter apenas letras, números, underscore e ponto")]
        [Display(Name = "Nome do Sistema")]
        public required string SystemName { get; set; }

        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres")]
        [Display(Name = "Descrição")]
        public required string Description { get; set; }
    }
}