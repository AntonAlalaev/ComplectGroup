namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель операции технологического маршрута детали для передачи данных
/// </summary>
public class PartOperationDto
{
    public int Id { get; set; }

    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string ChapterName { get; set; } = string.Empty;

    public int BrigadeId { get; set; }

    public string BrigadeName { get; set; } = string.Empty;

    public int OperationTypeId { get; set; }

    public string OperationTypeCode { get; set; } = string.Empty;

    public string OperationTypeName { get; set; } = string.Empty;

    public int SequenceNumber { get; set; }

    public decimal PlannedLaborHours { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }
}
