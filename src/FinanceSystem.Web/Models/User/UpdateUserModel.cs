using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.User
{
    public class UpdateUserModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome de Usuário")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*(),.?""{}|<>]).+$", ErrorMessage = "A senha deve conter pelo menos um caractere especial.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string? Password { get; set; }

        [Display(Name = "Perfis")]
        public List<string> Roles { get; set; } = [];

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
    }
}