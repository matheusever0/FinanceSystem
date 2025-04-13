using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class UpdateUserModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome de Usuário")]
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Display(Name = "Perfis")]
        public List<string> Roles { get; set; } = new List<string>();

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true; 
    }
}