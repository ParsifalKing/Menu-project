namespace Domain.DTOs.IngredientDTOs;

public class GetIngredientDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public double Count { get; set; }
    public decimal Price { get; set; }
    public bool IsInReserve { get; set; }
    public string? PathPhoto { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
