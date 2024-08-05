using Domain.DTOs.IngredientDTOs;

namespace Domain.DTOs.DrinkDTOs;

public class GetDrinkWithAllIngredients
{
    public required GetDrinkDto Drink { get; set; }
    public List<GetIngredientDto>? DrinkIngredients { get; set; }
}
