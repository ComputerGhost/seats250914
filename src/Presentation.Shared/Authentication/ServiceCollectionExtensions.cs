using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Shared.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyAuthentication(this IServiceCollection services, Action<AuthenticationOptions> configure)
    {
        services.Configure(configure);

        var myOptions = new AuthenticationOptions();
        configure(myOptions);

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = myOptions.LoginPath;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

        return services;
    }
}
