using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Login
{
    public class LoginModel
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [Display(Name = "Nome de Usuário")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public required string Password { get; set; }
    }
}