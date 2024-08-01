using Domain.DTOs.DishIngredientDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.DishIngredientService;

public interface IDishIngredientService
{
    Task<PagedResponse<List<GetDishIngredientDto>>> GetDishIngredientAsync(PaginationFilter filter);
    Task<Response<GetDishIngredientDto>> GetDishIngredientByIdAsync(DishIngredientDto dishIngredientDto);
    Task<Response<string>> CreateDishIngredientAsync(DishIngredientDto createCategory);
    Task<Response<string>> UpdateDishIngredientAsync(UpdateDishIngredientDto updateCategory);
    Task<Response<bool>> DeleteDishIngredientAsync(DishIngredientDto dishIngredientDto);
}
