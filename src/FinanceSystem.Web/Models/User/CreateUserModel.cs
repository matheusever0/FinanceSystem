using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.User
{
    public class CreateUserModel
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome de Usuário")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public required string Email { get; set; }

        [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*(),.?""{}|<>]).+$", ErrorMessage = "A senha deve conter pelo menos um caractere especial.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public required string Password { get; set; }

        [Display(Name = "Perfis")]
        public List<string> Roles { get; set; } = [];
    }
}