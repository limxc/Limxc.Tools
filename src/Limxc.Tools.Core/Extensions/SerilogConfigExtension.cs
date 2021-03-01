using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Limxc.Tools.Core.Extensions
{
    public static class SerilogConfigExtension
    {
        /// <summary>
        /// Log.Logger = new LoggerConfiguration().Default().CreateLogger();
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static LoggerConfiguration Default(this LoggerConfiguration configuration, string filePath = "Serilog.json")
        {
            if (File.Exists(filePath))
            {
                var cfg = new ConfigurationBuilder()
                //.SetBasePath(EnvPath.BaseDirectory)
                .AddJsonFile(filePath, true, true)
                .Build();
                return configuration.ReadFrom.Configuration(cfg);
            }

            return configuration
                        .Enrich.FromLogContext()
                        .WriteTo.Console(LogEventLevel.Debug, "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.Debug(LogEventLevel.Debug, "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.File
                             (
                                 path: "Logs\\.log",
                                 restrictedToMinimumLevel: LogEventLevel.Information,
                                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                                 formatProvider: null,
                                 fileSizeLimitBytes: 5242880,//5m
                                 levelSwitch: null,
                                 buffered: false,
                                 shared: true,//是否允许文件多进程共享(buffered:true时,不可共享)
                                 flushToDiskInterval: null,
                                 rollingInterval: RollingInterval.Day,
                                 rollOnFileSizeLimit: true,
                                 retainedFileCountLimit: 10
                                 )
                        ;
        }
    }
}