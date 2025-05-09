namespace Equilibrium.Web.Models.Filters
{
    public class UserFilter : BaseFilter
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public bool? IsActive { get; set; }
        public string RoleId { get; set; }
    }
}
