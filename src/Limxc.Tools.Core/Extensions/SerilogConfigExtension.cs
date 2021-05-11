using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Limxc.Tools.Core.Extensions
{
    public static class SerilogConfigExtension
    {
        /// <summary>
        ///     Log.Logger = new LoggerConfiguration().Default().CreateLogger();
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static LoggerConfiguration Default(this LoggerConfiguration configuration,
            string filePath = "Serilog.json")
        {
            if (File.Exists(filePath))
            {
                var cfg = new ConfigurationBuilder()
                    //.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(filePath, true, true)
                    .Build();
                return configuration.ReadFrom.Configuration(cfg);
            }

            return configuration
                    .Enrich.FromLogContext()
                    .WriteTo.Console(LogEventLevel.Debug,
                        "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.Debug(LogEventLevel.Debug,
                        "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File
                    (
                        "Logs\\.log",
                        LogEventLevel.Information,
                        "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        null,
                        5242880, //5m
                        null,
                        false,
                        true, //是否允许文件多进程共享(buffered:true时,不可共享)
                        null,
                        RollingInterval.Day,
                        true,
                        10
                    )
                ;
        }
    }
}