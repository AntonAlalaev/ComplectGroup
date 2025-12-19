using ComplectGroup.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models;

/// <summary>
/// ViewModel для отгрузки деталей по комплектации
/// </summary>
public class ComplectationShippingViewModel
{
    [Required(ErrorMessage = "Выберите комплектацию")]
    [Display(Name = "Комплектация")]
    public int? SelectedComplectationId { get; set; }

    /// <summary>
    /// Все доступные комплектации для dropdown
    /// </summary>
    public List<ComplectationDto> Complectations { get; set; } = new();

    /// <summary>
    /// Позиции выбранной комплектации с информацией об отгрузке
    /// </summary>
    public List<ShippingLineItem> LineItems { get; set; } = new();

    public class ShippingLineItem
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
        /// Уже отгруженное количество
        /// </summary>
        public int AlreadyShipped { get; set; }

        /// <summary>
        /// Осталось отгрузить (RequiredQuantity - AlreadyShipped)
        /// </summary>
        public int RemainingToShip => Math.Max(0, RequiredQuantity - AlreadyShipped);

        /// <summary>
        /// Остаток на складе
        /// </summary>
        public int WarehouseQuantity { get; set; }

        /// <summary>
        /// Количество к отгрузке (заполняется пользователем)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int ShippingQuantity { get; set; }
    }
}
