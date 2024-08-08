using Domain.DTOs.DishDTOs;

namespace Domain.DTOs.CategoryDTOs;

public class GetCategoryWithAllDishes
{
    public required GetCategoryDto Category { get; set; }
    public List<GetDishDto>? CategoryDishes { get; set; }
}
