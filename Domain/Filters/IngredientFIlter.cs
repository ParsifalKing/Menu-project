namespace Domain.Filters;

public class IngredientFIlter : PaginationFilter
{
    public string? Name { get; set; }
    public double? Count { get; set; }
    public decimal? Price { get; set; }
    public bool? IsInReserve { get; set; }
}
