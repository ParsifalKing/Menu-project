namespace Domain.Filters;

public class DrinkFilter : PaginationFilter
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
}
