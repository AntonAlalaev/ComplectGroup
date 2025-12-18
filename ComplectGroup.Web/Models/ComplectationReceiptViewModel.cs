using ComplectGroup.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models;

/// <summary>
/// ViewModel для приходования деталей по комплектации
/// </summary>
public class ComplectationReceiptViewModel
{
    [Required(ErrorMessage = "Выберите комплектацию")]
    [Display(Name = "Комплектация")]
    public int? SelectedComplectationId { get; set; }

    /// <summary>
    /// Все доступные комплектации для dropdown
    /// </summary>
    public List<ComplectationDto> Complectations { get; set; } = new();

    /// <summary>
    /// Позиции выбранной комплектации с количествами для приходования
    /// </summary>
    public List<ReceiptLineItem> LineItems { get; set; } = new();

    public class ReceiptLineItem
    {
        /// <summary>
        /// ID позиции комплектации
        /// </summary>
        public int PositionId { get; set; }

        /// <summary>
        /// ID детали
        /// </summary>
        public int PartId { get; set; }

        /// <summary>
        /// Название раздела
        /// </summary>
        public string Chapter { get; set; } = string.Empty;

        /// <summary>
        /// Название детали
        /// </summary>
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// Требуемое количество в комплектации
        /// </summary>
        public int RequiredQuantity { get; set; }

        /// <summary>
        /// Количество к приходованию (заполняется пользователем)
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int ReceiptQuantity { get; set; }
    }
}