using FinanceSystem.Web.Models.Login;
using FinanceSystem.Web.Models.User;

namespace FinanceSystem.Web.Services
{
    public interface IUserService
    {
        Task<LoginResponseModel> LoginAsync(LoginModel model);
        Task<IEnumerable<UserModel>> GetAllUsersAsync(string token);
        Task<UserModel> GetUserByIdAsync(string id, string token);
        Task<UserModel> CreateUserAsync(CreateUserModel model, string token);
        Task<UserModel> UpdateUserAsync(string id, UpdateUserModel model, string token);
        Task DeleteUserAsync(string id, string token);
    }
}