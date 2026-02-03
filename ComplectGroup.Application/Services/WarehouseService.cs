namespace ComplectGroup.Application.Services;

using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseItemRepository _warehouseRepo;
    private readonly IReceiptTransactionRepository _receiptRepo;
    private readonly IShippingTransactionRepository _shippingRepo;
    private readonly IPositionShipmentRepository _shipmentRepo;
    private readonly IPartRepository _partRepo;
    private readonly IPositionRepository _positionRepo;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IWarehouseItemRepository warehouseRepo,
        IReceiptTransactionRepository receiptRepo,
        IShippingTransactionRepository shippingRepo,
        IPositionShipmentRepository shipmentRepo,
        IPartRepository partRepo,
        IPositionRepository positionRepo,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepo = warehouseRepo;
        _receiptRepo = receiptRepo;
        _shippingRepo = shippingRepo;
        _shipmentRepo = shipmentRepo;
        _partRepo = partRepo;
        _positionRepo = positionRepo;
        _logger = logger;
    }

    // ===== СКЛАД =====
    public async Task<WarehouseItemDto?> GetWarehouseItemAsync(int partId, CancellationToken ct)
    {
        var item = await _warehouseRepo.GetByPartIdAsync(partId, ct);
        return item == null ? null : MapWarehouseItemToDto(item);
    }

    public async Task<List<WarehouseItemDto>> GetAllWarehouseItemsAsync(CancellationToken ct)
    {
        var items = await _warehouseRepo.GetAllAsync(ct);
        return items.Select(MapWarehouseItemToDto).ToList();
    }

    public async Task<List<WarehouseItemDto>> GetAvailableWarehouseItemsAsync(CancellationToken ct)
    {
        var items = await _warehouseRepo.GetAllAsync(ct);
        return items
            .Select(MapWarehouseItemToDto)
            .Where(dto => dto.TotalQuantity > 0) // ← Фильтрация только здесь
            .ToList();
    }

    // ===== ПРИЁМКА =====
    public async Task<ReceiptTransactionDto> ReceiveAsync(
        int partId,
        int quantity,
        string notes,
        CancellationToken ct)
    {
        // Проверяем существование Part
        var part = await _partRepo.GetByIdAsync(partId, ct)
            ?? throw new KeyNotFoundException($"Part с ID {partId} не найдена.");

        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть > 0");

        // Получаем или создаём WarehouseItem
        var warehouseItem = await _warehouseRepo.GetByPartIdAsync(partId, ct);

        if (warehouseItem == null)
        {
            warehouseItem = new WarehouseItem
            {
                PartId = partId,
                Part = null!,
                AvailableQuantity = quantity,
                ReservedQuantity = 0,
                LastModifiedDate = DateTime.Now
            };
            await _warehouseRepo.AddAsync(warehouseItem, ct);
        }
        else
        {
            warehouseItem.AvailableQuantity += quantity;
            warehouseItem.LastModifiedDate = DateTime.Now;
            warehouseItem.Part = null!;
            await _warehouseRepo.UpdateAsync(warehouseItem, ct);
        }

        // Создаём транзакцию приёмки - ТОЛЬКО FK, без Part!
        var transaction = new ReceiptTransaction
        {
            PartId = partId,    // ← только FK
            Part = null!,       // ← НЕ загружаем Part
            Quantity = quantity,
            ReceiptDate = DateTime.Now,
            Notes = notes
        };
        await _receiptRepo.AddAsync(transaction, ct);

        // Для маппинга используем загруженный part
        return MapReceiptToDtoWithPart(transaction, part);
    }

    private ReceiptTransactionDto MapReceiptToDtoWithPart(ReceiptTransaction transaction, Part part)
    {
        return new ReceiptTransactionDto
        {
            Id = transaction.Id,
            Quantity = transaction.Quantity,
            ReceiptDate = transaction.ReceiptDate,
            Notes = transaction.Notes,
            Part = new PartDto
            {
                Id = part.Id,
                Name = part.Name,
                Chapter = new ChapterDto
                {
                    Id = part.Chapter.Id,
                    Name = part.Chapter.Name
                }
            }
        };
    }


    public async Task<List<ReceiptTransactionDto>> GetReceiptHistoryByPartAsync(int partId, CancellationToken ct)
    {
        var transactions = await _receiptRepo.GetByPartIdAsync(partId, ct);
        return transactions.Select(MapReceiptToDto).ToList();
    }

    public async Task<List<ReceiptTransactionDto>> GetAllReceiptsAsync(CancellationToken ct)
    {
        var transactions = await _receiptRepo.GetAllAsync(ct);
        return transactions.Select(MapReceiptToDto).ToList();
    }

    // ===== ОТГРУЗКА =====
    public async Task<ShippingTransactionDto> ShipAsync(
        int partId,
        int quantity,
        int positionId,
        string notes,
        CancellationToken ct)
    {
        var part = await _partRepo.GetByIdAsync(partId, ct)
            ?? throw new KeyNotFoundException($"Part с ID {partId} не найдена.");

        var position = await _positionRepo.GetByIdAsync(positionId, ct)
            ?? throw new KeyNotFoundException($"Position с ID {positionId} не найдена.");

        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть > 0");

        var shipment = await _shipmentRepo.GetByPositionIdAsync(positionId, ct);
        int alreadyShipped = shipment?.ShippedQuantity ?? 0;

        // ===== ИЗМЕНЕНИЕ: Логируем предупреждение, но разрешаем отгрузку =====
        if (alreadyShipped + quantity > position.Quantity)
        {
            _logger.LogWarning($"Отгруженное количество превышает требуемое. " +
            $"Требуется: {position.Quantity}, уже отгружено: {alreadyShipped}, " +
            $"попытка отгрузить: {quantity}. Операция разрешена.");
        }

        // Проверяем только наличие на складе (это важно!)
        var warehouseItem = await _warehouseRepo.GetByPartIdAsync(partId, ct)
            ?? throw new InvalidOperationException($"На складе не найдена деталь {part.Name}");

        if (warehouseItem.AvailableQuantity < quantity)
            throw new InvalidOperationException(
                $"Недостаточно товара на складе. Доступно: {warehouseItem.AvailableQuantity}, " +
                $"требуется: {quantity}");
        
        // Обновляем склад
        warehouseItem.AvailableQuantity -= quantity;
        warehouseItem.LastModifiedDate = DateTime.Now;
        warehouseItem.Part = null!;
        await _warehouseRepo.UpdateAsync(warehouseItem, ct);
         
        // Обновляем PositionShipment
        if (shipment == null)
        {
            shipment = new PositionShipment
            {
                PositionId = positionId,
                Position = null!,
                ShippedQuantity = quantity,
                FirstShippedDate = DateTime.Now,
                LastShippedDate = DateTime.Now
            };
            await _shipmentRepo.AddAsync(shipment, ct);
        }
        else
        {
            shipment.ShippedQuantity += quantity;
            shipment.LastShippedDate = DateTime.Now;
            shipment.Position = null!;
            await _shipmentRepo.UpdateAsync(shipment, ct);
        }

        // Создаём транзакцию - ТОЛЬКО FK
        var transaction = new ShippingTransaction
        {
            PartId = partId,        // ← только FK
            Part = null!,           // ← НЕ загружаем
            PositionId = positionId,
            Position = null!,       // ← НЕ загружаем
            Quantity = quantity,
            ShippingDate = DateTime.Now,
            Notes = notes
        };
        await _shippingRepo.AddAsync(transaction, ct);

        _logger.LogInformation(
            "Отгрузка: {PartName} x{Quantity} для позиции {PositionId}. Примечание: {Notes}",
            part.Name, quantity, positionId, notes);

        return MapShippingToDtoWithPart(transaction, part);
    }

    private ShippingTransactionDto MapShippingToDtoWithPart(ShippingTransaction transaction, Part part)
    {
        return new ShippingTransactionDto
        {
            Id = transaction.Id,
            PositionId = transaction.PositionId,
            Quantity = transaction.Quantity,
            ShippingDate = transaction.ShippingDate,
            Notes = transaction.Notes,
            Part = new PartDto
            {
                Id = part.Id,
                Name = part.Name,
                Chapter = new ChapterDto
                {
                    Id = part.Chapter.Id,
                    Name = part.Chapter.Name
                }
            }
        };
    }


    public async Task<List<ShippingTransactionDto>> GetShippingHistoryByPositionAsync(int positionId, CancellationToken ct)
    {
        var transactions = await _shippingRepo.GetByPositionIdAsync(positionId, ct);
        return transactions.Select(MapShippingToDto).ToList();
    }

    public async Task<List<ShippingTransactionDto>> GetShippingHistoryByPartAsync(int partId, CancellationToken ct)
    {
        var transactions = await _shippingRepo.GetByPartIdAsync(partId, ct);
        return transactions.Select(MapShippingToDto).ToList();
    }

    public async Task<List<ShippingTransactionDto>> GetAllShippingsAsync(CancellationToken ct)
    {
        var transactions = await _shippingRepo.GetAllAsync(ct);
        return transactions.Select(MapShippingToDto).ToList();
    }

    public async Task<PositionShipmentDto?> GetPositionShipmentAsync(int positionId, CancellationToken ct)
    {
        var shipment = await _shipmentRepo.GetByPositionIdAsync(positionId, ct);
        return shipment == null ? null : MapPositionShipmentToDto(shipment);
    }

    // ===== Маппинг =====
    private WarehouseItemDto MapWarehouseItemToDto(WarehouseItem item)
    {
        return new WarehouseItemDto
        {
            Id = item.Id,
            AvailableQuantity = item.AvailableQuantity,
            ReservedQuantity = item.ReservedQuantity,
            LastModifiedDate = item.LastModifiedDate,
            Part = new PartDto
            {
                Id = item.Part.Id,
                Name = item.Part.Name,
                Chapter = new ChapterDto
                {
                    Id = item.Part.Chapter.Id,
                    Name = item.Part.Chapter.Name
                }
            }
        };
    }

    private ReceiptTransactionDto MapReceiptToDto(ReceiptTransaction transaction)
    {
        return new ReceiptTransactionDto
        {
            Id = transaction.Id,
            Quantity = transaction.Quantity,
            ReceiptDate = transaction.ReceiptDate,
            Notes = transaction.Notes,
            Part = new PartDto
            {
                Id = transaction.Part.Id,
                Name = transaction.Part.Name,
                Chapter = new ChapterDto
                {
                    Id = transaction.Part.Chapter.Id,
                    Name = transaction.Part.Chapter.Name
                }
            }
        };
    }

    private ShippingTransactionDto MapShippingToDto(ShippingTransaction transaction)
    {
        return new ShippingTransactionDto
        {
            Id = transaction.Id,
            PositionId = transaction.PositionId,
            Quantity = transaction.Quantity,
            ShippingDate = transaction.ShippingDate,
            Notes = transaction.Notes,
            Part = new PartDto
            {
                Id = transaction.Part.Id,
                Name = transaction.Part.Name,
                Chapter = new ChapterDto
                {
                    Id = transaction.Part.Chapter.Id,
                    Name = transaction.Part.Chapter.Name
                }
            }
        };
    }

    private PositionShipmentDto MapPositionShipmentToDto(PositionShipment shipment)
    {
        return new PositionShipmentDto
        {
            Id = shipment.Id,
            PositionId = shipment.PositionId,
            ShippedQuantity = shipment.ShippedQuantity,
            RequiredQuantity = shipment.Position.Quantity,            
            FirstShippedDate = shipment.FirstShippedDate,
            LastShippedDate = shipment.LastShippedDate
        };
    }
}
