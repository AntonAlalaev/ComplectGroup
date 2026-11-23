using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на создание/обновление операции технологического маршрута
/// </summary>
public class PartOperationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Выберите деталь")]
    public int PartId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите бригаду")]
    public int BrigadeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите тип операции")]
    public int OperationTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Порядковый номер должен быть больше 0")]
    public int SequenceNumber { get; set; }

    public decimal PlannedLaborHours { get; set; }

    [StringLength(2000, ErrorMessage = "Примечание не должно превышать 2000 символов")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
