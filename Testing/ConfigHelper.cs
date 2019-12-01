using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace Testing
{
    /// <summary>
    /// thanks to https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
    /// </summary>
    internal static class ConfigHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string fileName)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile(fileName)                                
                .Build();
        }
    }
}
