# ComplectGroup — Контекст проекта

## Обзор проекта

**ComplectGroup** — это ASP.NET Core 8.0 веб-приложение для управления комплектациями и складским учётом. Система позволяет управлять комплектациями продукции, отслеживать отгрузки, вести учёт товаров на складе с поддержкой ролевой модели доступа.

### Архитектура

Проект следует классической многослойной архитектуре:

```
ComplectGroup/
├── ComplectGroup.Domain/          # Доменный слой (сущности, бизнес-объекты)
├── ComplectGroup.Application/     # Слой приложения (сервисы, DTO, интерфейсы)
├── ComplectGroup.Infrastructure/  # Инфраструктурный слой (EF Core, репозитории, Identity)
└── ComplectGroup.Web/             # Презентационный слой (MVC контроллеры, Views)
```

### Технологический стек

- **Фреймворк**: .NET 8.0 (ASP.NET Core)
- **База данных**: SQLite через Entity Framework Core 8.0
- **Аутентификация**: ASP.NET Core Identity
- **Архитектурный паттерн**: Repository + Service Layer
- **UI**: MVC (Razor Views) + Swagger для API документации

---

## Структура проекта

### ComplectGroup.Domain
Доменные сущности бизнес-логики:
- `Complectation` — комплектация (основная сущность)
- `Position` — позиция в комплектации
- `Part` — деталь/товар
- `Chapter` — раздел/категория деталей
- `WarehouseItem` — остатки на складе
- `ReceiptTransaction` — приходные операции
- `ShippingTransaction` — расходные операции
- `CorrectionTransaction` — корректировки (пересортица)
- `PositionShipment` — отгрузки по позициям

### ComplectGroup.Application
Сервисы и DTO для бизнес-логики:
- **Services**: `ComplectationService`, `PartService`, `ChapterService`, `PositionService`, `WarehouseService`, `CorrectionService`
- **DTOs**: Транспортные объекты для всех сущностей
- **Interfaces**: Контракты для репозиториев и сервисов
- **Exceptions**: Пользовательские исключения

### ComplectGroup.Infrastructure
Инфраструктурная реализация:
- **Data**: `AppDbContext` (EF Core контекст с Identity)
- **Repositories**: Реализация репозиториев
- **Identity**: `ApplicationUser`, `ApplicationRole`
- **Migrations**: Миграции EF Core
- **Services**: Внешние сервисы

### ComplectGroup.Web
Веб-приложение:
- **Controllers**: MVC контроллеры + API контроллеры
- **Views**: Razor представления
- **Services**: Сервисы для контроллеров
- **Extensions**: Методы расширения
- `Program.cs` — точка входа и конфигурация DI

---

## Сборка и запуск

### Требования
- .NET 8.0 SDK
- Visual Studio 2022 или VS Code

### Команды

```bash
# Восстановление зависимостей
dotnet restore

# Сборка проекта
dotnet build

# Запуск приложения (из корня решения)
dotnet run --project ComplectGroup.Web

# Запуск с конкретным URL
dotnet run --project ComplectGroup.Web --urls "http://localhost:5215"
```

### Миграции базы данных

```bash
# Добавить новую миграцию
dotnet ef migrations add MigrationName --project ComplectGroup.Infrastructure --startup-project ComplectGroup.Web

# Обновить базу данных
dotnet ef database update --project ComplectGroup.Infrastructure --startup-project ComplectGroup.Web

# Удалить последнюю миграцию
dotnet ef migrations remove --project ComplectGroup.Infrastructure --startup-project ComplectGroup.Web
```

> **Примечание**: Миграции применяются автоматически при запуске приложения (см. `Program.cs`).

### Конфигурация

Основная конфигурация в `ComplectGroup.Web/appsettings.json`:
- **ConnectionStrings:DefaultConnection**: Путь к SQLite базе (`ComplectGroup.db`)
- **Kestrel**: Порт приложения (по умолчанию `http://localhost:5215`)

---

## Ролевая модель и авторизация

### Роли пользователей
| Роль | Описание |
|------|----------|
| `Administrator` | Полный доступ ко всем функциям |
| `Manager` | Почти полный доступ (кроме управления деталями) |
| `Operator` | Базовые складские операции |
| `Viewer` | Только просмотр |

### Политики авторизации

**Ролевые политики:**
- `RequireAdministrator` — только администраторы
- `RequireManager` — администраторы и менеджеры
- `RequireOperator` — администраторы, менеджеры, операторы
- `RequireViewer` — все роли

**Функциональные политики:**
- `CanViewWarehouse` — просмотр склада
- `CanReceive` — приходование товара
- `CanShip` — отгрузка товара
- `CanCorrect` — корректировка пересортицы
- `CanManageComplectations` — управление комплектациями
- `CanManageParts` — управление деталями и разделами

---

## Ключевые зависимости

| Пакет | Версия | Назначение |
|-------|--------|------------|
| `Microsoft.EntityFrameworkCore` | 8.0.* | ORM |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.0 | Identity |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.* | Провайдер БД |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger/OpenAPI |
| `EPPlusFree` | 4.5.3.8 | Работа с Excel |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.0 | UI для Identity |

---

## Разработка

### Соглашения по коду
- **Nullable reference types**: Включены (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Включены (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Язык**: C# с использованием современных возможностей .NET 8

### Структура слоёв
1. **Domain** — только сущности и value objects, без зависимостей
2. **Application** — бизнес-логика, интерфейсы репозиториев
3. **Infrastructure** — реализация репозиториев, EF Core, внешние сервисы
4. **Web** — контроллеры, представления, конфигурация

### Тестирование
На момент анализа тестовые проекты отсутствуют. Рекомендуется добавить:
- Unit-тесты для сервисов слоя Application
- Integration-тесты для API контроллеров

---

## База данных

### Основные таблицы
- `Complectations` — комплектации
- `Positions` — позиции комплектаций
- `Parts` — детали/товары
- `Chapters` — разделы деталей
- `WarehouseItems` — складские остатки
- `ReceiptTransactions` — приход
- `ShippingTransactions` — расход
- `CorrectionTransactions` — корректировки
- `AspNetUsers`, `AspNetRoles` — Identity

---

## API Endpoints

Swagger доступен в режиме разработки по адресу: `http://localhost:5215/swagger`

API контроллеры (префикс `Api`):
- `ApiChaptersController` — операции с разделами
- `ApiComplectationsController` — операции с комплектациями
- `ApiPartsController` — операции с деталями

---

## Примечания

- Приложение использует SQLite базу данных `ComplectGroup.db` в корне веб-проекта
- Identity автоматически создаёт роли и пользователя при запуске (см. `SeedData.Initialize`)
- HTTPS редирект включён в конфигурации
- CORS настроен с политикой `AllowAll` для разработки
