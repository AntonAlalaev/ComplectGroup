// ComplectGroup.Application/DTOs/CorrectionTransactionDto.cs
namespace ComplectGroup.Application.DTOs;

public class CorrectionTransactionDto
{
    public int Id { get; set; }
    public string CorrectionNumber { get; set; } = string.Empty;
    public PartDto OldPart { get; set; } = default!;
    public PartDto NewPart { get; set; } = default!;
    public int Quantity { get; set; }
    public DateTime CorrectionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}