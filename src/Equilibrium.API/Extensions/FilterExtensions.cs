using Equilibrium.Application.DTOs.Common;

namespace Equilibrium.API.Extensions
{
    public static class FilterExtensions
    {
        public static void AddPaginationHeaders<T>(this HttpResponse response, PagedResult<T> pagedResult)
        {
            response.Headers.Add("X-Pagination-Total", pagedResult.TotalCount.ToString());
            response.Headers.Add("X-Pagination-Pages", pagedResult.TotalPages.ToString());
            response.Headers.Add("X-Pagination-Page", pagedResult.PageNumber.ToString());
            response.Headers.Add("X-Pagination-Size", pagedResult.PageSize.ToString());
            response.Headers.Add("Access-Control-Expose-Headers", 
                "X-Pagination-Total, X-Pagination-Pages, X-Pagination-Page, X-Pagination-Size");
        }
    }
}
