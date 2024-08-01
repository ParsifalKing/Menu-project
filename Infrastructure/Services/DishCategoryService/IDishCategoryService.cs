using Domain.DTOs.DishCategoryDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.DishCategoryService;

public interface IDishCategoryService
{
    Task<PagedResponse<List<GetDishCategoryDto>>> GetDishCategoryAsync(PaginationFilter filter);
    Task<Response<GetDishCategoryDto>> GetDishCategoryByIdAsync(DishCategoryDto dishCategoryDto);
    Task<Response<string>> CreateDishCategoryAsync(DishCategoryDto createCategory);
    Task<Response<string>> UpdateDishCategoryAsync(UpdateDishCategoryDto updateCategory);
    Task<Response<bool>> DeleteDishCategoryAsync(DishCategoryDto dishCategoryDto);
}
