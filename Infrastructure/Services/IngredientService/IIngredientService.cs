using Domain.DTOs.IngredientDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.IngredientService;

public interface IIngredientService
{
    Task<PagedResponse<List<GetIngredientDto>>> GetIngredientsAsync(IngredientFIlter filter);
    Task<Response<GetIngredientDto>> GetIngredientByIdAsync(Guid ingredientId);
    Task<Response<string>> CreateIngredientAsync(CreateIngredientDto createIngredient);
    Task<Response<string>> UpdateIngredientAsync(UpdateIngredientDto updateIngredient);
    Task<Response<bool>> DeleteIngredientAsync(Guid ingredientId);
}
