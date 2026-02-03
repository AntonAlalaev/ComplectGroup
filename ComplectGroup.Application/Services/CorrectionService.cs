// ComplectGroup.Application/Services/CorrectionService.cs
namespace ComplectGroup.Application.Services;
using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;

public class CorrectionService : ICorrectionService
{
    private readonly IWarehouseService _warehouseService;
    private readonly ICorrectionTransactionRepository _correctionRepo;
    private readonly IPartRepository _partRepo;
    private readonly ILogger<CorrectionService> _logger;
    
    public CorrectionService(
        IWarehouseService warehouseService,
        ICorrectionTransactionRepository correctionRepo,
        IPartRepository partRepo,
        ILogger<CorrectionService> logger)
    {
        _warehouseService = warehouseService;
        _correctionRepo = correctionRepo;
        _partRepo = partRepo;
        _logger = logger;
    }
    
    public async Task<CorrectionTransactionDto> CreateCorrectionAsync(
        int oldPartId, 
        int newPartId, 
        int quantity, 
        string notes, 
        CancellationToken ct)
    {
        // Валидация
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть больше 0");
        
        if (oldPartId == newPartId)
            throw new ArgumentException("Старая и новая детали должны быть разными");
        
        // Проверяем наличие старой детали на складе
        var oldPartWarehouse = await _warehouseService.GetWarehouseItemAsync(oldPartId, ct);
        if (oldPartWarehouse == null || oldPartWarehouse.AvailableQuantity < quantity)
        {
            // ИСПРАВЛЕНО: используем другое имя переменной, чтобы избежать конфликта
            var oldPartInfo = await _partRepo.GetByIdAsync(oldPartId, ct);
            throw new InvalidOperationException(
                $"Недостаточно деталей '{oldPartInfo?.Name ?? "Неизвестно"}' на складе. " +
                $"Доступно: {oldPartWarehouse?.AvailableQuantity ?? 0}");
        }
        
        // Получаем информацию о деталях
        // ИСПРАВЛЕНО: переменные уже объявлены здесь, не нужно повторно объявлять
        var oldPart = await _partRepo.GetByIdAsync(oldPartId, ct)
            ?? throw new KeyNotFoundException($"Старая деталь с ID {oldPartId} не найдена");
        
        var newPart = await _partRepo.GetByIdAsync(newPartId, ct)
            ?? throw new KeyNotFoundException($"Новая деталь с ID {newPartId} не найдена");
        
        // Выполняем списание старой детали (отгрузка)
        var shippingNotes = $"Корректировка пересортицы: списание {quantity} шт. (старая деталь: {oldPart.Name})";
        await _warehouseService.ShipAsync(
            oldPartId,
            quantity,
            0, // PositionId = 0 для корректировок (не привязано к комплектации)
            shippingNotes,
            ct);
        
        _logger.LogInformation(
            "Списание старой детали при корректировке: {OldPartName} x{Quantity}",
            oldPart.Name, quantity);
        
        // Выполняем приход новой детали (приёмка)
        var receiptNotes = $"Корректировка пересортицы: приход {quantity} шт. (новая деталь: {newPart.Name})";
        await _warehouseService.ReceiveAsync(
            newPartId,
            quantity,
            receiptNotes,
            ct);
        
        _logger.LogInformation(
            "Приход новой детали при корректировке: {NewPartName} x{Quantity}",
            newPart.Name, quantity);
        
        // Генерируем уникальный номер корректировки
        var correctionNumber = await GenerateCorrectionNumberAsync(ct);
        
        // Создаем запись о корректировке
        // ИСПРАВЛЕНО: добавляем обязательные навигационные свойства (как в ShippingTransaction)
        var correction = new CorrectionTransaction
        {
            CorrectionNumber = correctionNumber,
            OldPartId = oldPartId,
            OldPart = null!, // ← Устанавливаем в null! как в других транзакциях
            NewPartId = newPartId,
            NewPart = null!, // ← Устанавливаем в null! как в других транзакциях
            Quantity = quantity,
            CorrectionDate = DateTime.Now,
            Notes = notes,
            CreatedBy = "System" // TODO: Получить текущего пользователя
        };
        
        await _correctionRepo.AddAsync(correction, ct);
        
        _logger.LogInformation(
            "Корректировка пересортицы создана: {CorrectionNumber}, {OldPartName} -> {NewPartName}, {Quantity} шт.",
            correctionNumber, oldPart.Name, newPart.Name, quantity);
        
        // Возвращаем DTO
        return new CorrectionTransactionDto
        {
            Id = correction.Id,
            CorrectionNumber = correction.CorrectionNumber,
            OldPart = MapPartToDto(oldPart),
            NewPart = MapPartToDto(newPart),
            Quantity = correction.Quantity,
            CorrectionDate = correction.CorrectionDate,
            Notes = correction.Notes,
            CreatedBy = correction.CreatedBy
        };
    }
    
    private async Task<string> GenerateCorrectionNumberAsync(CancellationToken ct)
    {
        // Формат: CORR-ГГГГ-ННН (например: CORR-2026-001)
        var year = DateTime.Now.Year;
        var lastCorrection = await _correctionRepo.GetLastAsync(ct);
        var nextNumber = (lastCorrection?.Id ?? 0) + 1;
        return $"CORR-{year}-{nextNumber:D3}";
    }
    
    private PartDto MapPartToDto(Part part)
    {
        return new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Chapter = new ChapterDto
            {
                Id = part.Chapter.Id,
                Name = part.Chapter.Name
            }
        };
    }
    
    public async Task<List<CorrectionTransactionDto>> GetCorrectionHistoryAsync(CancellationToken ct)
    {
        var corrections = await _correctionRepo.GetAllAsync(ct);
        var result = new List<CorrectionTransactionDto>();
        
        foreach (var correction in corrections)
        {
            var oldPart = await _partRepo.GetByIdAsync(correction.OldPartId, ct);
            var newPart = await _partRepo.GetByIdAsync(correction.NewPartId, ct);
            
            if (oldPart == null || newPart == null)
                continue;
            
            result.Add(new CorrectionTransactionDto
            {
                Id = correction.Id,
                CorrectionNumber = correction.CorrectionNumber,
                OldPart = MapPartToDto(oldPart),
                NewPart = MapPartToDto(newPart),
                Quantity = correction.Quantity,
                CorrectionDate = correction.CorrectionDate,
                Notes = correction.Notes,
                CreatedBy = correction.CreatedBy
            });
        }
        
        return result;
    }
}