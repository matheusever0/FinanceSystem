namespace Equilibrium.Web.Models.Commons
{
    public class DeleteButtonModel
    {
        public string Id { get; set; } = "";
        public string Controller { get; set; } = "";
        public string EntityName { get; set; } = "";
        public string Description { get; set; } = "";
        public string CssClass { get; set; } = "btn btn-outline-danger";
        public string CustomMessage { get; set; } = "";
        public string RedirectUrl { get; set; } = "";
        public bool CheckStatus { get; set; } = false;
        public string StatusField { get; set; } = "Status";
        public string[]? AllowedStatuses { get; set; } = null;
        public bool ShowConfirmation { get; set; } = true;
        public bool IconOnly { get; set; } = false;
        public string ButtonText { get; set; } = "";
    }
}
