using Public.Features.Localization.Models;

namespace Public.Features.Localization.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyLocalization(this IServiceCollection services, Action<LocalizationOptions> configure)
    {
        // Stash in config for later use in app building and components.
        services.Configure(configure);

        // Of course we need the options here too.
        var options = new LocalizationOptions();
        configure(options);

        if (options.UseViewLocalization)
        {
            // It's fine if AddMvc has already been called. We'll call it again.
            services.AddMvc().AddViewLocalization();
        }

        if (options.UseLocalizationResources)
        {
            services.AddMvc().AddViewLocalization(); // For IHtmlLocalizer
            services.AddLocalization(options =>
            {
                // It's pretty much standard to put resource files here.
                options.ResourcesPath = "Resources";
            });
        }

        return services;
    }
}
