using System.Diagnostics;
using System.IO; // Required for potential stream operations (e.g., if reading request body)
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder; // Необходим для IApplicationBuilder
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing; // Необходим для GetRouteData()
using Microsoft.Extensions.Logging; // Необходим для ILogger

namespace BookLibrary.Middleware // Убедитесь, что это пространство имен соответствует вашему проекту
{
    /// <summary>
    /// Middleware для логирования входящих HTTP-запросов и времени их обработки.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger; // Внедряем ILogger

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RequestLoggingMiddleware"/>.
        /// </summary>
        /// <param name="next">Следующий делегат запроса в конвейере.</param>
        /// <param name="logger">Интерфейс логирования для вывода информации.</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Асинхронно вызывает middleware для обработки HTTP-запроса.
        /// </summary>
        /// <param name="context">Текущий HTTP-контекст.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // Логирование начала обработки запроса с использованием структурированного логирования
            _logger.LogInformation(
                "[{MiddlewareName}] Начало обработки запроса: {RequestMethod} {RequestScheme}://{RequestHost}{RequestPath}{RequestQueryString}",
                nameof(RequestLoggingMiddleware), // Имя middleware для идентификации
                request.Method,
                request.Scheme,
                request.Host,
                request.Path,
                request.QueryString);

            // Опционально: Если вам нужно прочитать тело запроса, вам нужно будет включить буферизацию.
            // Это важно, так как поток тела запроса обычно может быть прочитан только один раз.
            // request.EnableBuffering();
            // var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            // request.Body.Position = 0; // Перемотать поток в начало для следующего middleware/контроллера

            // Передача управления следующему компоненту в конвейере
            await _next(context);

            stopwatch.Stop();

            // Получение данных о маршруте после того, как он был определен конвейером маршрутизации
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();
            var actionName = routeData?.Values["action"]?.ToString();
            var routeInfo = (controllerName != null && actionName != null) ? $"{controllerName}/{actionName}" : "N/A";

            // Опционально: Для логирования тела ответа вам потребуется перехватить поток ответа.
            // Это более сложная задача и обычно не реализуется в простом middleware для логирования
            // из-за потенциального влияния на производительность.

            // Логирование завершения обработки запроса
            _logger.LogInformation(
                "[{MiddlewareName}] Завершение обработки запроса. Длительность: {DurationMs}мс. Маршрут: {RouteInfo}. Статус: {StatusCode}",
                nameof(RequestLoggingMiddleware),
                stopwatch.ElapsedMilliseconds,
                routeInfo,
                context.Response.StatusCode);
        }
    }
    /// <summary>
    /// Класс-расширение для удобного добавления <see cref="RequestLoggingMiddleware"/> в конвейер запросов приложения.
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        /// <summary>
        /// Добавляет <see cref="RequestLoggingMiddleware"/> в конвейер запросов приложения.
        /// </summary>
        /// <param name="builder">Экземпляр <see cref="IApplicationBuilder"/>.</param>
        /// <returns>Экземпляр <see cref="IApplicationBuilder"/> для цепочки вызовов.</returns>
        public static IApplicationBuilder UseRequestLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
