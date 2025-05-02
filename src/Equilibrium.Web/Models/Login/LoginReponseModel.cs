using Equilibrium.Web.Models.User;

namespace Equilibrium.Web.Models.Login
{
    public class LoginResponseModel
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public required UserModel User { get; set; }
    }
}