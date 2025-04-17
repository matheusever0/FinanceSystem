using FinanceSystem.Web.Models.User;

namespace FinanceSystem.Web.Models.Login
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserModel User { get; set; }
    }
}