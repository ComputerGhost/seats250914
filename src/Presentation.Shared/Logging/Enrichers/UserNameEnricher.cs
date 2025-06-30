using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Presentation.Shared.Logging.Enrichers;
internal class UserNameEnricher : ILogEventEnricher
{
    private const string UserIdPropertyName = "UserName";

    private readonly IHttpContextAccessor _contextAccessor;

    public UserNameEnricher() : this(new HttpContextAccessor())
    {
    }

    public UserNameEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_contextAccessor.HttpContext == null)
            return;

        var userId = _contextAccessor.HttpContext.User.Identity?.Name;

        var correlationIdProperty = new LogEventProperty(UserIdPropertyName, new ScalarValue(userId));

        logEvent.AddOrUpdateProperty(correlationIdProperty);
    }
}
