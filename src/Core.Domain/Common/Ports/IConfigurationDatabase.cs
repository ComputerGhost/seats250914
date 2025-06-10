using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface IConfigurationDatabase
{
    /// <summary>
    /// Fetches the latest configuration. If not found, return <see cref="ConfigurationEntityModel.Default"/>.
    /// </summary>
    Task<ConfigurationEntityModel> FetchConfiguration();

    Task<bool> SaveConfiguration(ConfigurationEntityModel configuration);
}
