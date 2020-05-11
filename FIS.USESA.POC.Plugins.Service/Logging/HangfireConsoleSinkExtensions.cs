﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hangfire.Server;
using Serilog;

namespace FIS.USESA.POC.Plugins.Service.Logging
{
    public static class HangfireConsoleSinkExtensions
    {
        public static ILogger CreateLoggerForPerformContext<T>(this PerformContext context)
        {
            return Log.ForContext<T>()
                .ForContext(new HangfireConsoleSerilogEnricher { PerformContext = context });
        }
    }
}
