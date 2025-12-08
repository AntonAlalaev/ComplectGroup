using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Services;
using ComplectGroup.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext регистрация и конфигурация
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                         ?? "Data Source=ComplectGroup.db";

    options.UseSqlite(connectionString);
});

// 2. Repository регистрация
builder.Services.AddScoped<IComplectationRepository, ComplectationRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();

// 3. Service регистрация
builder.Services.AddScoped<IComplectationService, ComplectationService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IComplectationImportService, ComplectationImportService>();

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

var app = builder.Build();

// ============ DATABASE MIGRATION ON STARTUP ============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrated successfully!");
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

app.UseHttpsRedirection();
app.UseStaticFiles();          // если будут стили/скрипты
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

// ============ ROUTE CONFIGURATION ============

// MVC и API контроллеры (конвенция маршрутизация)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
