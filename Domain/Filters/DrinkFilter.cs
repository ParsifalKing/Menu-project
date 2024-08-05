namespace Domain.Filters;

public class DrinkFilter : PaginationFilter
{
    public string? Name { get; set; }
    public bool? AreAllIngredients { get; set; }
    public string? IngredientName { get; set; }
    public int? CookingTimeInMinutes { get; set; }
    public decimal? Price { get; set; }

}
