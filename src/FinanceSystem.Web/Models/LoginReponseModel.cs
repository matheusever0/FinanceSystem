namespace FinanceSystem.Web.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserModel User { get; set; }
    }
}