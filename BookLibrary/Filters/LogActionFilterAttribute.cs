using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookLibrary.Filters // Убедитесь, что пространство имен соответствует вашему проекту
{
    public class LogActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            string controllerName = actionDescriptor?.ControllerName ?? "UnknownController";
            string actionName = actionDescriptor?.ActionName ?? "UnknownAction";
            string methodType = context.HttpContext.Request.Method;

            Console.WriteLine($"[ActionFilter] Executing action: {controllerName}/{actionName} (Method: {methodType})");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            string controllerName = actionDescriptor?.ControllerName ?? "UnknownController";
            string actionName = actionDescriptor?.ActionName ?? "UnknownAction";

            if (context.Exception == null)
            {
                Console.WriteLine($"[ActionFilter] Finished executing action: {controllerName}/{actionName}");
            }
            else
            {
                // Если исключение произошло, но было обработано (например, ExceptionFilter),
                // или если оно не обработано здесь, мы это отмечаем.
                // Сам GlobalExceptionFilter позаботится о логировании деталей исключения.
                Console.WriteLine($"[ActionFilter] Finished executing action: {controllerName}/{actionName} with exception: {context.Exception.GetType().Name}");
            }
            base.OnActionExecuted(context);
        }
    }
}
