using BookLibrary.Repositories;
using BookLibrary.Middleware; // Добавляем using для нашего Middleware
using BookLibrary.Filters;    // Добавляем using для наших фильтров

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Регистрация репозиториев (как и ранее)
builder.Services.AddSingleton<IAuthorRepository, JsonAuthorRepository>();
builder.Services.AddSingleton<IBookRepository, JsonBookRepository>();
builder.Services.AddSingleton<IReaderRepository, JsonReaderRepository>();

// Добавление контроллеров с представлениями и конфигурация глобальных фильтров
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogActionFilterAttribute>();       // Глобальное добавление фильтра действий
    options.Filters.Add<GlobalExceptionFilterAttribute>(); // Глобальное добавление фильтра исключений
});
// Если бы GlobalExceptionFilterAttribute принимал ILogger через DI:
// builder.Services.AddScoped<GlobalExceptionFilterAttribute>(); // Зарегистрировать фильтр как сервис
// options.Filters.AddService<GlobalExceptionFilterAttribute>(); // И добавить его как сервис


var app = builder.Build();

// Configure the HTTP request pipeline.

// **Регистрация нашего кастомного Middleware**
// Обычно размещается в начале конвейера, чтобы перехватывать все запросы.
// Если Middleware нужно точное знание маршрута в самом начале InvokeAsync,
// его можно разместить после app.UseRouting(), но для текущей задачи
// (логирование Path в начале и Route в конце) текущее размещение подходит.
app.UseRequestLogging();

if (!app.Environment.IsDevelopment())
{
    // Этот обработчик будет менее востребован, если GlobalExceptionFilter перехватывает все
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // В режиме разработки можно оставить стандартную страницу ошибок для некоторых случаев
    // app.UseDeveloperExceptionPage(); // Или убрать, если GlobalExceptionFilter покрывает все нужды
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); // Важно: UseRouting должен быть до UseEndpoints (MapControllerRoute)
                  // и до Middleware, если оно хочет получить доступ к RouteData сразу

app.UseAuthorization(); // Если используется авторизация

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
