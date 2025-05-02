namespace Equilibrium.Web.Models.Shared
{
    public class CollapseSectionModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string PartialName { get; set; }
        public bool InitiallyExpanded { get; set; } = false;
    }
}