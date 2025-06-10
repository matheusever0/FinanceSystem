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

        public DeleteButtonModel()
        {
            Id = "";
            Controller = "";
            EntityName = "";
            Description = "";
            CssClass = "btn btn-outline-danger";
            CustomMessage = "";
            RedirectUrl = "";
            CheckStatus = false;
            StatusField = "Status";
            AllowedStatuses = null;
            ShowConfirmation = true;
            IconOnly = false;
            ButtonText = "";
        }

        public DeleteButtonModel(string id, string controller, string entityName, string description, bool iconOnly = false)
        {
            Id = id;
            Controller = controller;
            EntityName = entityName;
            Description = description;
            CssClass = iconOnly ? "btn btn-sm btn-danger" : "btn btn-outline-danger w-100";
            CustomMessage = "";
            RedirectUrl = "";
            CheckStatus = false;
            StatusField = "Status";
            AllowedStatuses = null;
            ShowConfirmation = true;
            IconOnly = iconOnly;
            ButtonText = iconOnly ? "" : $"Excluir {entityName}";
        }
    }
}