using Domain.DTOs.CategoryDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.CategoryService;

public interface ICategoryService
{
    Task<PagedResponse<List<GetCategoryDto>>> GetCategoriesAsync(CategoryFilter filter);
    Task<Response<GetCategoryDto>> GetCategoryByIdAsync(Guid categoryId);
    Task<Response<string>> CreateCategoryAsync(CreateCategoryDto createCategory);
    Task<Response<string>> UpdateCategoryAsync(UpdateCategoryDto updateCategory);
    Task<Response<bool>> DeleteCategoryAsync(Guid categoryId);
}
