using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;


/// <summary>
/// Сервис для работы с комплектациями
/// </summary>
public class ComplectationService : IComplectationService
{
    /// <summary>
    /// Репозиторий комплектаций
    /// </summary>
    private readonly IComplectationRepository _complectationRepository;
    
    /// <summary>
    /// Репозиторий деталей (для валидации)
    /// </summary>
    private readonly IPartRepository _partRepository;
    
    private readonly IWarehouseService _warehouseService;

    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<ComplectationService> _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    public ComplectationService(
        IComplectationRepository complectationRepository,
        IPartRepository partRepository,
        IWarehouseService warehouseService,
        ILogger<ComplectationService> logger)
    {
        _complectationRepository = complectationRepository;
        _partRepository = partRepository;
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Получение комплектации по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<ComplectationDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var complectation = await _complectationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Комплектация с ID {id} не найдена");

        return MapToDto(complectation);
    }

    /// <summary>
    /// Возвращает все комплектации в формате DTO
    /// </summary>
    public async Task<List<ComplectationDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var complectations = await _complectationRepository.GetAllAsync(cancellationToken);
        return complectations.Select(MapToDto).ToList();
    }


    /// <summary>
    /// Получить комплектации, которые ещё не полностью отгружены
    /// </summary>
    public async Task<List<ComplectationDto>> GetNotFullyShippedAsync(CancellationToken cancellationToken)
    {
        var complectations = await _complectationRepository.GetAllAsync(cancellationToken);
        
        // Фильтруем те, статус которых не FullyShipped
        var notFullyShipped = complectations
            .Where(c => c.Status != ComplectationStatus.FullyShipped)
            .ToList();

        return notFullyShipped.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Создает новую комплектацию
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<ComplectationDto> CreateAsync(CreateComplectationRequest request, CancellationToken cancellationToken)
    {
        // Валидация номера
        if (string.IsNullOrWhiteSpace(request.Number))
            throw new ArgumentException("Номер комплектации обязателен.");

        // Проверяем существование всех PartId
        var partIds = request.Positions.Select(p => p.PartId).Distinct().ToList();
        var parts = await _partRepository.GetAllAsync(cancellationToken);
        var partDict = parts.ToDictionary(p => p.Id);

        foreach (var partId in partIds)
        {
            if (!partDict.ContainsKey(partId))
                throw new KeyNotFoundException($"Деталь с ID {partId} не найдена.");
        }

        // Создаём позиции
        var positions = request.Positions.Select(posRequest =>
        {
            // валидация partDict уже сделана выше
            return new Position
            {
                PartId = posRequest.PartId,          // ✅ только FK
                Quantity = posRequest.Quantity
                // Part не трогаем
            };
        }).ToList();

        // Создаём комплектацию: TotalWeight и TotalVolume берём из запроса
        var complectation = new Complectation
        {
            Number = request.Number,
            Manager = request.Manager,
            Address = request.Address,
            Customer = request.Customer,
            ShippingDate = request.ShippingDate,
            CreatedDate = request.CreatedDate ?? DateOnly.FromDateTime(DateTime.Today),
            ShippingTerms = request.ShippingTerms,
            TotalWeight = request.TotalWeight,     
            TotalVolume = request.TotalVolume,    
            Positions = positions
        };

        await _complectationRepository.AddAsync(complectation, cancellationToken);

        _logger.LogInformation("Создана комплектация: {Number}", complectation.Number);

        return MapToDto(complectation);
    }

    /// <summary>
    /// Обновляет комплектацию
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task UpdateAsync(int id, UpdateComplectationRequest request, CancellationToken cancellationToken)
    {
        var complectation = await _complectationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Комплектация с ID {id} не найдена.");

        // Обновляем поля, если они заданы
        if (!string.IsNullOrWhiteSpace(request.Number))
            complectation.Number = request.Number;

        if (!string.IsNullOrWhiteSpace(request.Manager))
            complectation.Manager = request.Manager;

        if (!string.IsNullOrWhiteSpace(request.Address))
            complectation.Address = request.Address;

        if (!string.IsNullOrWhiteSpace(request.Customer))
            complectation.Customer = request.Customer;

        if (request.ShippingDate.HasValue)
            complectation.ShippingDate = request.ShippingDate.Value;

        if (request.ShippingTerms != null)
            complectation.ShippingTerms = request.ShippingTerms;

        // ✅ Обновляем вес и объём, если указаны
        if (request.TotalWeight.HasValue)
            complectation.TotalWeight = request.TotalWeight.Value;

        if (request.TotalVolume.HasValue)
            complectation.TotalVolume = request.TotalVolume.Value;

        // Обновляем позиции, если указаны
        if (request.Positions != null)
        {
            await UpdatePositionsAsync(complectation, request.Positions, cancellationToken);
        }

        // 🔁 НЕ пересчитываем вес и объём — они вводятся вручную
        await _complectationRepository.UpdateAsync(complectation, cancellationToken);

        _logger.LogInformation("Обновлена комплектация: {Number}", complectation.Number);
    }


    /// <summary>
    /// Удаляет комплектацию по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var complectation = await _complectationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Комплектация с ID {id} не найдена.");

        await _complectationRepository.DeleteAsync(complectation, cancellationToken);

        _logger.LogInformation("Удалена комплектация: {Number}", complectation.Number);
    }

    // ===== МЕТОДЫ ДЛЯ ПРОВЕРКИ ПОЛНОТЫ ОТГРУЗКИ =====

    /// <summary>
    /// Проверить, полностью ли отгружена комплектация
    /// </summary>
    public async Task<bool> IsFullyShippedAsync(int complectationId, CancellationToken cancellationToken)
    {
        var complectation = await _complectationRepository.GetByIdAsync(complectationId, cancellationToken);
        
        if (complectation == null)
            throw new KeyNotFoundException($"Комплектация с ID {complectationId} не найдена");

        // Если уже отмечена как полностью отгруженная
        if (complectation.Status == ComplectationStatus.FullyShipped)
            return true;

        // Если нет позиций — не может быть полностью отгружена
        if (!complectation.Positions.Any())
            return false;

        // Получаем все отгрузки
        var allShippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);

        // Проверяем каждую позицию
        foreach (var position in complectation.Positions)
        {
            var requiredQuantity = position.Quantity;
            var shippedQuantity = allShippings
                .Where(s => s.PositionId == position.Id)
                .Sum(s => s.Quantity);

            // Если хоть одна позиция не полностью отгружена — комплектация не готова
            if (shippedQuantity < requiredQuantity)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Отметить комплектацию как полностью отгруженную
    /// </summary>
    public async Task MarkAsFullyShippedAsync(int complectationId, CancellationToken cancellationToken)
    {
        var complectation = await _complectationRepository.GetByIdAsync(complectationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Комплектация с ID {complectationId} не найдена");

        // Проверяем, действительно ли полностью отгружена
        var isFullyShipped = await IsFullyShippedAsync(complectationId, cancellationToken);

        if (!isFullyShipped)
        {
            _logger.LogWarning($"Попытка отметить комплектацию {complectation.Number} как полностью отгруженную, " +
                             $"но она ещё не полностью отгружена");
            throw new InvalidOperationException($"Комплектация {complectation.Number} ещё не полностью отгружена");
        }

        complectation.Status = ComplectationStatus.FullyShipped;
        complectation.FullyShippedDate = DateTime.Now;

        await _complectationRepository.UpdateAsync(complectation, cancellationToken);
        _logger.LogInformation($"Комплектация {complectation.Number} отмечена как полностью отгруженная. " +
                             $"Дата: {complectation.FullyShippedDate:dd.MM.yyyy HH:mm}");
    }







    // --- Вспомогательные методы ---

    /// <summary>
    /// Обновляет позиции комплектации
    /// </summary>
    /// <param name="complectation"></param>
    /// <param name="requests"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    private async Task UpdatePositionsAsync(Complectation complectation, List<UpdatePositionRequest> requests, CancellationToken ct)
    {
        var existingPositions = complectation.Positions.ToDictionary(p => p.Id);
        var allParts = await _partRepository.GetAllAsync(ct);
        var partDict = allParts.ToDictionary(p => p.Id);
        var newPositions = new List<Position>();

        foreach (var req in requests)
        {
            if (req.IsDeleted == true && req.Id.HasValue && existingPositions.ContainsKey(req.Id.Value))
            {
                // просто не добавляем её в newPositions => будет удалена
                continue;
            }

            if (req.Id.HasValue && existingPositions.TryGetValue(req.Id.Value, out var existingPos))
            {
                // обновляем существующую
                if (req.PartId.HasValue)
                {
                    if (!partDict.ContainsKey(req.PartId.Value))
                        throw new KeyNotFoundException($"Деталь с ID {req.PartId.Value} не найдена.");

                    existingPos.PartId = req.PartId.Value;      // ✅ только FK
                }

                if (req.Quantity.HasValue && req.Quantity.Value > 0)
                    existingPos.Quantity = req.Quantity.Value;

                newPositions.Add(existingPos);
            }
            else
            {
                // новая позиция
                if (!req.PartId.HasValue || !req.Quantity.HasValue || req.Quantity <= 0)
                    throw new ArgumentException("Для новой позиции необходимо указать PartId и Quantity > 0.");

                if (!partDict.ContainsKey(req.PartId.Value))
                    throw new KeyNotFoundException($"Деталь с ID {req.PartId.Value} не найдена.");

                var newPos = new Position
                {
                    PartId = req.PartId.Value,                  // ✅ только FK
                    Quantity = req.Quantity.Value
                };

                newPositions.Add(newPos);
            }
        }

        complectation.Positions.Clear();
        complectation.Positions.AddRange(newPositions);
    }
  
    /// <summary>
    /// Маппинг из сущности в DTO
    /// </summary>
    /// <param name="complectation"></param>
    /// <returns></returns>
    private ComplectationDto MapToDto(Complectation complectation)
    {
        return new ComplectationDto
        {
            Id = complectation.Id,
            Number = complectation.Number,
            Manager = complectation.Manager,
            Address = complectation.Address,
            Customer = complectation.Customer,
            ShippingDate = complectation.ShippingDate,
            CreatedDate = complectation.CreatedDate,
            ShippingTerms = complectation.ShippingTerms,
            TotalWeight = complectation.TotalWeight,
            TotalVolume = complectation.TotalVolume,
            Status = (int)complectation.Status,            
            Positions = complectation.Positions.Select(p =>
            {
                var part = p.Part;
                var chapter = part?.Chapter;

                return new PositionDto
                {
                    Id = p.Id,
                    Quantity = p.Quantity,
                    Part = new PartDto
                    {
                        // хотя бы FK, если навигация null
                        Id = part?.Id ?? p.PartId,
                        Name = part?.Name ?? $"[Part ID {p.PartId}]",
                        Chapter = new ChapterDto
                        {
                            Id = chapter?.Id ?? 0,
                            Name = chapter?.Name ?? string.Empty
                        }
                    }
                };
            }).ToList()
        };
    }


}
