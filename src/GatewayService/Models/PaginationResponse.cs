namespace GatewayService.Models;

public class PaginationResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalElements { get; set; } // Было TotalCount
    public List<T> Items { get; set; } = new();
}