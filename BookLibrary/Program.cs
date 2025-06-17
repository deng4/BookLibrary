using BookLibrary.Repositories;
using BookLibrary.Middleware;
using BookLibrary.Filters;
using Microsoft.EntityFrameworkCore;
using BookLibrary.Models.DatabaseModels;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавление DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Регистрация репозиториев с новой реализацией
// Scoped - жизненный цикл совпадает с жизненным циклом запроса, что хорошо для DbContext
builder.Services.AddScoped<IAuthorRepository, EfAuthorRepository>();
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IReaderRepository, EfReaderRepository>();

// Добавление контроллеров, фильтров и т.д. (как раньше)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogActionFilterAttribute>();
    options.Filters.Add<GlobalExceptionFilterAttribute>();
});

var app = builder.Build();

// **(Опционально, но рекомендуется) Автоматическое применение миграций при старте**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LibraryDbContext>();
        context.Database.Migrate(); // Применяет все непримененные миграции
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}


// Configure the HTTP request pipeline.
app.UseRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
