using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BlockOrderControl
{
    [DefaultValue(1)]
    public int Id { get; set; }
    public bool IsBlocked { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
