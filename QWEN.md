# ComplectGroup — Контекст проекта

## Обзор проекта

**ComplectGroup** — это ASP.NET Core 8.0 веб-приложение для управления комплектациями и складским учётом. Система позволяет управлять комплектациями продукции, отслеживать отгрузки, вести учёт товаров на складе с поддержкой ролевой модели доступа.

### Архитектура

Проект следует классической многослойной архитектуре:

```
ComplectGroup/
├── ComplectGroup.Domain/          # Доменный слой (сущности, бизнес-объекты)
├── ComplectGroup.Application/     # Слой приложений (сервисы, DTO, интерфейсы)
├── ComplectGroup.Infrastructqweure/  # Инфраструктурный слой (EF Core, репозитории, Identity)
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
- `ComplectationStatus` — enum статусов комплектации

### ComplectGroup.Application
Сервисы и DTO для бизнес-логики:
- **Services**: `ComplectationService`, `PartService`, `ChapterService`, `PositionService`, `WarehouseService`, `CorrectionService`
- **DTOs**: Транспортные объекты для всех сущностей
- **Interfaces**: Контракты для репозиториев и сервисов
- **Exceptions**: Пользовательские исключения
- **Models**: ViewModel для фильтрации и пагинации

### ComplectGroup.Infrastructure
Инфраструктурная реализация:
- **Data**: `AppDbContext` (EF Core контекст с Identity)
- **Repositories**: Реализация репозиториев
- **Identity**: `ApplicationUser`, `ApplicationRole`
- **Services**: `SeedData` (инициализация ролей и пользователей), `ClaimsTransformer`
- **Migrations**: Миграции EF Core

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
- **ConnectionStrings:DefaultConnection**: Путь к SQLite базе (`Data Source=ComplectGroup.db`)
- **Kestrel**: Порт приложения (по умолчанию `http://localhost:5215`)

---

## Ролевая модель и авторизация

### Роли пользователей
| Роль | Описание |
|------|----------|
| `Administrator` | Полный доступ ко всем функциям |
| `Manager` | Почти полный доступ (кроме управления деталями) |
| `SeniorOperator` | Склад + коррективы |
| `Operator` | Базовые складские операции |
| `Guest` | Только просмотр |

### Права (Permissions)
| Право | Описание |
|-------|----------|
| `CanView` | Просмотр всех данных |
| `CanIgnoreComplectations` | Игнорирование комплектаций |
| `CanReceive` | Приходование товара |
| `CanShip` | Отгрузка товара |
| `CanCorrect` | Корректировка пересортицы |
| `CanImportComplectations` | Загрузка из Excel |
| `CanEditComplectations` | Редактирование комплектаций |
| `CanDeleteComplectations` | Удаление комплектаций |
| `CanManageParts` | Управление деталями |
| `CanManageChapters` | Управление разделами |

### Политики авторизации

**Claims-политики:**
- `CanView` — базовый доступ для всех
- `CanReceive` — приходование
- `CanShip` — отгрузка
- `CanCorrect` — корректировки
- `CanImportComplectations` — импорт Excel
- `CanEditComplectations` — редактирование
- `CanDeleteComplectations` — удаление
- `CanManageParts` — управление деталями
- `CanManageChapters` — управление разделами

**Ролевые политики:**
- `RequireAdministrator` — только администраторы
- `RequireManager` — администраторы и менеджеры
- `RequireSeniorOperator` — администраторы, менеджеры, старшие операторы
- `RequireOperator` — все роли

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
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.0 | Логирование |

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
- `PositionShipments` — отгрузки по позициям
- `CorrectionTransactions` — корректировки пересортицы
- `AspNetUsers`, `AspNetRoles` — Identity

### Миграции
- `20251205083110_InitialCreate` — начальная структура
- `20251215093142_AddWarehouseAndTransactions` — склад и транзакции
- `20251219070058_AddComplectationStatusAndFullyShippedDate` — статусы комплектаций
- `20251226092445_AddIdentityTables` — Identity
- `20260203093508_AddCorrectionTransaction` — корректировки
- `20260218120016_AddIsIgnoredToComplectation` — флаг игнорирования

---

## API Endpoints

Swagger доступен в режиме разработки по адресу: `http://localhost:5215/swagger`

API контроллеры (префикс `Api`):
- `ApiChaptersController` — операции с разделами
- `ApiComplectationsController` — операции с комплектациями
- `ApiPartsController` — операции с деталями

---

## Тестовые учётные данные

После запуска приложения доступны следующие пользователи:

| Роль | Email | Пароль |
|------|-------|--------|
| Administrator | admin@complectgroup.com | Admin123! |
| Manager | manager@complectgroup.com | Manager123! |
| SeniorOperator | senior@complectgroup.com | Senior123! |
| Operator | operator@complectgroup.com | Operator123! |
| Guest | guest@complectgroup.com | Guest123! |

---

## Примечания

- Приложение использует SQLite базу данных `ComplectGroup.db` в корне веб-проекта
- Identity автоматически создаёт роли и пользователей при запуске (см. `SeedData.Initialize`)
- HTTPS редирект включён в конфигурации
- CORS настроен с политикой `AllowAll` для разработки
- Статусы комплектаций: `Draft`, `PartiallyShipped`, `FullyShipped`, `Archived`
