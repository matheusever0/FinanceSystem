using FinanceSystem.Web.Models.User;

namespace FinanceSystem.Web.Models.Login
{
    public class LoginResponseModel
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public required UserModel User { get; set; }
    }
}