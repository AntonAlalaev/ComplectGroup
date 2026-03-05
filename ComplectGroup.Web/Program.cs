using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Services;
using ComplectGroup.Infrastructure.Repositories;
using ComplectGroup.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext регистрация и конфигурация
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                         ?? "Data Source=ComplectGroup.db";

    options.UseSqlite(connectionString);
});

// 2. Repository регистрация - основные таблицы сущностей, которые были до склада
builder.Services.AddScoped<IComplectationRepository, ComplectationRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();

// отгрузка и приход - для склада - то что добавилось со складом
builder.Services.AddScoped<IPositionShipmentRepository, PositionShipmentRepository>();
builder.Services.AddScoped<IReceiptTransactionRepository, ReceiptTransactionRepository>();
builder.Services.AddScoped<IShippingTransactionRepository, ShippingTransactionRepository>();
builder.Services.AddScoped<IWarehouseItemRepository, WarehouseItemRepository>();
builder.Services.AddScoped<ICorrectionTransactionRepository, CorrectionTransactionRepository>();

// user management 
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// 2.1. Регистрация Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
    //.AddDefaultUI(); // Если хотите стандартные страницы входа/регистрации

// 2.2. Настройка параметров Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Пароль
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Пользователь
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Блокировка
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
});

// 2.3. Настройка Cookie аутентификации
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

// 2.3.1. Регистрация Claims Transformer для загрузки прав из БД
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

// 2.4. Добавление политик авторизации (после AddControllersWithViews)
// ===== НОВАЯ СИСТЕМА ПОЛИТИК НА ОСНОВЕ CLAIMS =====
builder.Services.AddAuthorization(options =>
{
    // ===== БАЗОВЫЕ ПОЛИТИКИ =====

    // Просмотр всех данных (базовый доступ для всех зарегистрированных пользователей)
    options.AddPolicy("CanView", policy =>
        policy.RequireAuthenticatedUser());

    // Игнорирование комплектаций (доступно всем зарегистрированным)
    options.AddPolicy("CanIgnoreComplectations", policy =>
        policy.RequireAuthenticatedUser());

    // ===== СКЛАДСКИЕ ОПЕРАЦИИ =====

    // Приходование товара на склад
    options.AddPolicy("CanReceive", policy =>
        policy.RequireClaim("Permission", "CanReceive"));

    // Отгрузка товара со склада
    options.AddPolicy("CanShip", policy =>
        policy.RequireClaim("Permission", "CanShip"));

    // Корректировка пересортицы
    options.AddPolicy("CanCorrect", policy =>
        policy.RequireClaim("Permission", "CanCorrect"));

    // ===== УПРАВЛЕНИЕ КОМПЛЕКТАЦИЯМИ =====

    // Загрузка комплектаций из Excel
    options.AddPolicy("CanImportComplectations", policy =>
        policy.RequireClaim("Permission", "CanImportComplectations"));

    // Создание и редактирование комплектаций
    options.AddPolicy("CanEditComplectations", policy =>
        policy.RequireClaim("Permission", "CanEditComplectations"));

    // Удаление комплектаций
    options.AddPolicy("CanDeleteComplectations", policy =>
        policy.RequireClaim("Permission", "CanDeleteComplectations"));

    // ===== УПРАВЛЕНИЕ СПРАВОЧНИКАМИ =====

    // Управление деталями (создание, редактирование, удаление)
    options.AddPolicy("CanManageParts", policy =>
        policy.RequireClaim("Permission", "CanManageParts"));

    // Управление разделами (создание, редактирование, удаление)
    options.AddPolicy("CanManageChapters", policy =>
        policy.RequireClaim("Permission", "CanManageChapters"));

    // ===== РОЛЕВЫЕ ПОЛИТИКИ (для обратной совместимости) =====

    // Администратор - полный доступ
    options.AddPolicy("RequireAdministrator", policy =>
        policy.RequireRole("Administrator"));

    // Менеджер - почти полный доступ
    options.AddPolicy("RequireManager", policy =>
        policy.RequireRole("Administrator", "Manager"));

    // Старший оператор - склад + коррективы
    options.AddPolicy("RequireSeniorOperator", policy =>
        policy.RequireRole("Administrator", "Manager", "SeniorOperator"));

    // Оператор - базовые складские операции
    options.AddPolicy("RequireOperator", policy =>
        policy.RequireRole("Administrator", "Manager", "SeniorOperator", "Operator"));
});

// 3. Service регистрация - основные сервисы для работы с сущностями
builder.Services.AddScoped<IComplectationService, ComplectationService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IComplectationImportService, ComplectationImportService>();

// то что добавилось для склада
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ICorrectionService, CorrectionService>();

// Сервис прав (permissions)
builder.Services.AddScoped<IPermissionService, PermissionService>();

// WEB API & MVC & Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (если нужен для фронтенда)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============ APPLICATION BUILDER ============ Асинхронная инициализация
await using var app = builder.Build();

// ============ DATABASE MIGRATION ON STARTUP ============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrated successfully!");
        // Инициализация ролей и пользователей
        await SeedData.Initialize(scope.ServiceProvider);
        Console.WriteLine("✅ Identity data seeded successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
    }
}

// ============ MIDDLEWARE CONFIGURATION ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ComplectGroup API v1");
        // options.RoutePrefix = string.Empty; // Swagger на главной странице
    });
}

app.UseHttpsRedirection(); // если используется HTTPS
app.UseStaticFiles();          // если будут стили/скрипты
app.UseCors("AllowAll"); // если нужен CORS
app.UseRouting(); // для MVC и API

// ВАЖНО: Добавьте эту строку перед UseAuthorization
app.UseAuthentication();

app.UseAuthorization();

// ============ ROUTE CONFIGURATION ============

// MVC и API контроллеры (конвенция маршрутизация)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    
    // Если используете стандартные страницы Identity (Razor Pages)
    //app.MapRazorPages();

app.Run();
