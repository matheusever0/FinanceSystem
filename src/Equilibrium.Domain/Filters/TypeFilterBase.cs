namespace Equilibrium.Domain.DTOs.Filters
{
    public class TypeFilterBase : BaseFilter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsSystem { get; set; }
    }
}
