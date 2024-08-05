namespace Domain.DTOs.DishDTOs;

public class GetDishDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public required float Calorie { get; set; }
    public int CookingTimeInMinutes { get; set; }
    public bool AreAllIngredients { get; set; }
    public string? PathPhoto { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
