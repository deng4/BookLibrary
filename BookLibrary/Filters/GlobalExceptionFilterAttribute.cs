using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
// Для ILogger, если бы он использовался:
// using Microsoft.Extensions.Logging;
// using Microsoft.AspNetCore.Hosting; // Для IWebHostEnvironment
// using Microsoft.Extensions.DependencyInjection; // Для GetService

namespace BookLibrary.Filters // Убедитесь, что пространство имен соответствует вашему проекту
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        // Если бы использовался ILogger:
        // private readonly ILogger<GlobalExceptionFilterAttribute> _logger;
        // private readonly IWebHostEnvironment _env;

        // public GlobalExceptionFilterAttribute(ILogger<GlobalExceptionFilterAttribute> logger, IWebHostEnvironment env)
        // {
        //     _logger = logger;
        //     _env = env;
        // }

        // Конструктор без зависимостей для простоты, как в требовании
        public GlobalExceptionFilterAttribute() { }

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var controllerName = context.ActionDescriptor.RouteValues["controller"]?.ToString() ?? "UnknownController";
            var actionName = context.ActionDescriptor.RouteValues["action"]?.ToString() ?? "UnknownAction";

            // Логирование исключения
            Console.WriteLine($"[ExceptionFilter] Unhandled exception in {controllerName}/{actionName}: {exception.GetType().Name} - {exception.Message}");
            Console.WriteLine($"[ExceptionFilter] StackTrace: {exception.StackTrace}");
            // Пример с ILogger:
            // _logger.LogError(exception, $"[ExceptionFilter] Unhandled exception in {controllerName}/{actionName}");

            // Формирование JSON-ответа
            var errorResponse = new
            {
                message = "Произошла непредвиденная ошибка на сервере.",
                // В рабочей среде details можно сделать более общим или убрать
                // if (_env.IsDevelopment()) { details = exception.ToString(); } else { details = "Internal server error."; }
                details = exception.Message, // Для демонстрации показываем сообщение исключения
                timestamp = DateTime.UtcNow
            };

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError // Статус 500
            };

            // Помечаем исключение как обработанное
            context.ExceptionHandled = true;
        }
    }
}
