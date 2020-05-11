using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using FIS.USESA.POC.Plugins.Shared.Attributes;
using FIS.USESA.POC.Plugins.Shared.Entities;
using FIS.USESA.POC.Plugins.Shared.Interfaces;
using static FIS.USESA.POC.Plugins.Shared.Constants.SchedulerConstants;

namespace FIS.USAESA.Plugins.Autopay
{
    [JobPlugIn(Name = @"AutoPay", Version = 1.0)]
    public class AutoPayEventPublisher : IPlugIn
    {
        KafkaServiceConfigBE _kafkaConfig;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AutoPayEventPublisher()
        {
            // load plug-in specific configuration from appsettings.json file copied into the plug-in specific subfolder 
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", false)
                .Build();

            // read config values (the plug-in would use this information to connect to the correct DB to gather addl data required 
            string dbConnString = configuration.GetConnectionString("DataConnection");
        }

        public StdTaskReturnValueBE Execute(string jobId)
        {
            return new StdTaskReturnValueBE() { StepStatus = STD_STEP_STATUS.SUCCESS, ReturnMessage = "Ok" };
        }

        public string GetPlugInInfo()
        {
            var plugInAttribute = this.GetType().GetCustomAttribute<JobPlugInAttribute>();

            var compileTimestamp = typeof(AutoPayEventPublisher)
                                            .Assembly
                                            .GetCustomAttributes<AssemblyMetadataAttribute>()
                                            .First(a => a.Key == "CompileTimestamp")
                                            .Value;

            return $"Name: [{plugInAttribute.Name}], Version: [{plugInAttribute.Version}], Compiled: [{compileTimestamp}]";
        }

        /// <summary>
        /// Injects the generic configuration known by the hosting process.
        /// </summary>
        /// <param name="kafkaConfig">The configuration information.</param>
        public void InjectConfig(KafkaServiceConfigBE kafkaConfig)
        {
            _kafkaConfig = kafkaConfig;
        }
    }
}
