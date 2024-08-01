namespace Domain.DTOs.DishCategoryDTOs;

public class UpdateDishCategoryDto
{
    public required Guid Id { get; set; }
    public required Guid DishId { get; set; }
    public required Guid CategoryId { get; set; }
}
