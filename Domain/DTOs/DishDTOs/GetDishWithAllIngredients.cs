using Domain.DTOs.IngredientDTOs;

namespace Domain.DTOs.DishDTOs;

public class GetDishWithAllIngredients
{
    public required GetDishDto Dish { get; set; }
    public List<GetIngredientDto>? DishIngredients { get; set; }
}
