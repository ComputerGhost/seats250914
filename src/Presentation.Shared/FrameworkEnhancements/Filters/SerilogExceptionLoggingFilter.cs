using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Presentation.Shared.FrameworkEnhancements.Filters;
public class SerilogExceptionLoggingFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        Log.Error(context.Exception, "An unhandled exception occurred during request processing.");

        // Intentionally not marking the exception as handled.
        // > context.ExceptionHandled = true;
    }
}
