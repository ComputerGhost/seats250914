using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.Shared.FrameworkEnhancements.Filters;
public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationEx)
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = "Validation failed",
                details = validationEx.Message
            });

            context.ExceptionHandled = true;
        }
    }
}
