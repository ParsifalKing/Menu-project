using Domain.DTOs.DishIngredientDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.DishIngredientService;

public interface IDishIngredientService
{
    Task<PagedResponse<List<GetDishIngredientDto>>> GetDishIngredientAsync(PaginationFilter filter);
    Task<Response<GetDishIngredientDto>> GetDishIngredientByIdAsync(Guid dishId, Guid ingredientId);
    Task<Response<string>> CreateDishIngredientAsync(DishIngredientDto createDishIgredient);
    Task<Response<string>> UpdateDishIngredientAsync(UpdateDishIngredientDto updateDishIgredient);
    Task<Response<bool>> DeleteDishIngredientAsync(Guid dishId, Guid ingredientId);
}
