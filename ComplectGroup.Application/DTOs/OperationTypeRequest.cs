using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на создание/обновление типа операции
/// </summary>
public class OperationTypeRequest
{
    [Required(ErrorMessage = "Код операции обязателен")]
    [StringLength(50, ErrorMessage = "Код операции не должен превышать 50 символов")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Название операции обязательно")]
    [StringLength(200, ErrorMessage = "Название операции не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;
}
