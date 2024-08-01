namespace Domain.Filters;

public class DishFilter : PaginationFilter
{
    public bool? AreAllIngredients { get; set; }
    public string? CategoryName { get; set; }
    public string? IngredientName { get; set; }
    public string? CookingTime { get; set; }
    public decimal? Price { get; set; }

}
