namespace Equilibrium.Web.Models.Commons
{
    public class ActionButtonsModel
    {
        public string Id { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string ControllerName { get; set; }
        public DeleteButtonModel DeleteConfig { get; set; }
        public string ExtraButtons { get; set; }

        public ActionButtonsModel()
        {
        }

        public ActionButtonsModel(string id, bool canView, bool canEdit, bool canDelete,
            string controllerName, DeleteButtonModel deleteConfig = null, string extraButtons = "")
        {
            Id = id;
            CanView = canView;
            CanEdit = canEdit;
            CanDelete = canDelete;
            ControllerName = controllerName;
            DeleteConfig = deleteConfig;
            ExtraButtons = extraButtons ?? "";
        }
    }
}