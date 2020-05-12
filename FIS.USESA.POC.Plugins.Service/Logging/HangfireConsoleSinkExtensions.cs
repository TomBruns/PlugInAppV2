using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hangfire.Server;
using Serilog;

namespace FIS.USESA.POC.Plugins.Service.Logging
{
    /// <summary>
    /// Get the logger for the Hangfire Context
    /// </summary>
    public static class HangfireConsoleSinkExtensions
    {
        /// <summary>
        /// Get the logger for the Hangfire Context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ILogger CreateLoggerForPerformContext<T>(this PerformContext context)
        {
            return Log.ForContext<T>()
                .ForContext(new HangfireConsoleSerilogEnricher { PerformContext = context });
        }
    }
}
