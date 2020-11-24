using Limxc.Tools.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Limxc.Tools.Core.Extensions
{
    public static class SerilogConfigExtension
    {
        /// <summary>
        /// Log.Logger = new LoggerConfiguration().Default().CreateLogger();
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static LoggerConfiguration Default(this LoggerConfiguration configuration)
        {
            var config = new ConfigurationBuilder()
                        .SetBasePath(EnvPath.BaseDirectory)
                        .AddJsonFile("SerilogLogger.json", true, true)
                        .Build();
             
            return configuration.ReadFrom.Configuration(config);
        } 
    }
}