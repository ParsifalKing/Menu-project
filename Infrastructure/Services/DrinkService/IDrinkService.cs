using Domain.DTOs.DrinkDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.DrinkService;

public interface IDrinkService
{
    Task<PagedResponse<List<GetDrinkDto>>> GetDrinksAsync(DrinkFilter filter);
    Task<Response<GetDrinkWithAllIngredients>> GetDrinkByIdAsync(Guid drinkId);
    Task<Response<string>> CreateDrinkAsync(CreateDrinkDto createDrink);
    Task<Response<string>> UpdateDrinkAsync(UpdateDrinkDto updateDrink);
    Task<Response<bool>> DeleteDrinkAsync(Guid drinkId);
}
