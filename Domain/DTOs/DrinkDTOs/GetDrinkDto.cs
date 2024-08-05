namespace Domain.DTOs.DrinkDTOs;

public class GetDrinkDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int CookingTimeInMinutes { get; set; }
    public bool AreAllIngredients { get; set; }
    public string? PathPhoto { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
