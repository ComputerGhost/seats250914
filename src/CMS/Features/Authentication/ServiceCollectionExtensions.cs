namespace CMS.Features.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyAuthentication(this IServiceCollection services, Action<AuthenticationOptions> configure)
    {
        services.Configure(configure);

        var myOptions = new AuthenticationOptions();
        configure(myOptions);

        services.AddAuthentication()
            .AddCookie(options =>
            {
                options.LoginPath = myOptions.LoginPath;
            });

        return services;
    }
}
