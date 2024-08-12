using Microsoft.AspNetCore.Http;

namespace Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? PathPhoto { get; set; }

    public List<DishCategory>? Dishes { get; set; }
}
