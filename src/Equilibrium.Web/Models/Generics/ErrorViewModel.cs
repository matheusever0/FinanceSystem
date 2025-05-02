namespace Equilibrium.Web.Models.Generics
{
    public class ErrorViewModel
    {
        public required string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}