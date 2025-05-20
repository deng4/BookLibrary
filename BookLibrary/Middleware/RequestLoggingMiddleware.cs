using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing; // Required for GetRouteData()
// Для ILogger, если бы он использовался:
// using Microsoft.Extensions.Logging;

namespace BookLibrary.Middleware // Убедитесь, что пространство имен соответствует вашему проекту
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        // Если бы использовался ILogger:
        // private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next) // , ILogger<RequestLoggingMiddleware> logger
        {
            _next = next;
            // _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // Логирование начала обработки запроса
            Console.WriteLine($"[Middleware] Handling request: {request.Method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
            // Пример с ILogger:
            // _logger.LogInformation($"[Middleware] Handling request: {request.Method} {request.Path}");

            // Передача управления следующему компоненту в конвейере
            await _next(context);

            stopwatch.Stop();

            // Получение данных о маршруте после того, как он был определен
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();
            var actionName = routeData?.Values["action"]?.ToString();
            var routeInfo = (controllerName != null && actionName != null) ? $"{controllerName}/{actionName}" : "N/A";

            // Логирование завершения обработки запроса
            Console.WriteLine($"[Middleware] Finished handling request. Duration: {stopwatch.ElapsedMilliseconds}ms. Route: {routeInfo}. Status: {context.Response.StatusCode}");
            // Пример с ILogger:
            // _logger.LogInformation($"[Middleware] Finished handling request. Route: {routeInfo}. Status: {context.Response.StatusCode}");
        }
    }

    // Класс-расширение для удобного добавления Middleware в Program.cs
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
