using Microsoft.Extensions.Configuration;

namespace Testing
{
    /// <summary>
    /// thanks to https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
    /// </summary>
    internal static class ConfigHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("settings.json", optional: true)                                
                .Build();
        }
    }
}
