using Domain.DTOs.DrinkIngredientDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.DrinkIngredientService;

public interface IDrinkIngredientService
{
    Task<PagedResponse<List<GetDrinkIngredientDto>>> GetDrinkIngredientAsync(PaginationFilter filter);
    Task<Response<GetDrinkIngredientDto>> GetDrinkIngredientByIdAsync(Guid drinkId, Guid ingredientId);
    Task<Response<string>> CreateDrinkIngredientAsync(DrinkIngredientDto createDrinkIngredient);
    Task<Response<string>> UpdateDrinkIngredientAsync(UpdateDrinkIngredientDto updateDrinkIngredient);
    Task<Response<bool>> DeleteDrinkIngredientAsync(Guid drinkId, Guid ingredientId);
}
