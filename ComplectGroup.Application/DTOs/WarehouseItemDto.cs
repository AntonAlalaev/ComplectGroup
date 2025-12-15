namespace ComplectGroup.Application.DTOs;

public class WarehouseItemDto
{
    public int Id { get; set; }
    public PartDto Part { get; set; } = default!;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime LastModifiedDate { get; set; }

    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
}
