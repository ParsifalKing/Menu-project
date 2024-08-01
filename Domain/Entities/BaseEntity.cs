using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}