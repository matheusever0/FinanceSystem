namespace Equilibrium.Domain.DTOs.Filters
{
    public abstract class BaseFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "CreatedAt";
        public bool Ascending { get; set; } = false;
    }
}
