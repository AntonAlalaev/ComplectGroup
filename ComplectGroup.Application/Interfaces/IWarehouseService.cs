namespace ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.DTOs;

public interface IWarehouseService
{
    // ===== СКЛАД =====
    /// <summary>Получить текущее состояние товара на складе</summary>
    Task<WarehouseItemDto?> GetWarehouseItemAsync(int partId, CancellationToken ct);

    /// <summary>Получить все товары на складе</summary>
    Task<List<WarehouseItemDto>> GetAllWarehouseItemsAsync(CancellationToken ct);

    // ===== ПРИЁМКА =====
    /// <summary>Принять товар на склад (без привязки к позиции)</summary>
    Task<ReceiptTransactionDto> ReceiveAsync(
        int partId,
        int quantity,
        string notes,
        CancellationToken ct);

    /// <summary>Получить историю приёмок по детали</summary>
    Task<List<ReceiptTransactionDto>> GetReceiptHistoryByPartAsync(int partId, CancellationToken ct);

    /// <summary>Получить все приёмки</summary>
    Task<List<ReceiptTransactionDto>> GetAllReceiptsAsync(CancellationToken ct);

    // ===== ОТГРУЗКА =====
    /// <summary>Отгрузить товар (обязательна привязка к позиции)</summary>
    Task<ShippingTransactionDto> ShipAsync(
        int partId,
        int quantity,
        int positionId,
        string notes,
        CancellationToken ct);

    /// <summary>Получить историю отгрузок по позиции</summary>
    Task<List<ShippingTransactionDto>> GetShippingHistoryByPositionAsync(int positionId, CancellationToken ct);

    /// <summary>Получить историю отгрузок по детали</summary>
    Task<List<ShippingTransactionDto>> GetShippingHistoryByPartAsync(int partId, CancellationToken ct);

    /// <summary>Получить все отгрузки</summary>
    Task<List<ShippingTransactionDto>> GetAllShippingsAsync(CancellationToken ct);

    // ===== ОТСЛЕЖИВАНИЕ ПОЗИЦИИ =====
    /// <summary>Получить информацию об отгрузках позиции</summary>
    Task<PositionShipmentDto?> GetPositionShipmentAsync(int positionId, CancellationToken ct);
}
