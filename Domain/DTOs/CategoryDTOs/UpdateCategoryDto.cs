namespace Domain.DTOs.CategoryDTOs;

public class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
