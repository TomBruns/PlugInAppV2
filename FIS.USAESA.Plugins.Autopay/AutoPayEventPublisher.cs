﻿using System;
using System.Collections.Generic;
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
    /// <summary>
    /// This is a custom plug-in for the AutoPay Product
    /// It will be called on some schedule by the scheduler service, its role is to determine all of the events that need
    /// to be executed and publish a message to a Kafka Topic to be handled downstream by the AutoPay Service
    /// </summary>
    /// <remarks>
    /// The Name and Version attribute allow side-by-side deployment inside the scheduler because each plug-in is
    /// isolated in different AssemblyLoadContexts
    /// </remarks>
    [JobPlugIn(Name = @"AutoPay", Version = 1.0)]
    public class AutoPayEventPublisher : IPlugIn
    {
        KafkaServiceConfigBE _kafkaServiceConfig;
        KafkaPluginConfigBE _kafkaPlugInConfig;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AutoPayEventPublisher()
        {
            // load plug-in specific configuration from appsettings.json file copied into the plug-in specific subfolder 
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appSettings.json", false)
                .Build();

            // read config values (the plug-in would use this information to connect to the correct DB to gather addl data required 
            string dbConnString = configuration.GetConnectionString("DataConnection");

            _kafkaPlugInConfig = configuration.GetSection("KafkaPlugInConfig").Get<KafkaPluginConfigBE>(); ;

        }

        /// <summary>
        /// This is the method executed by the scheduler
        /// </summary>
        /// <param name="jobId">This is the Hangfire JobId that useful for debugging if you log it to the target DB</param>
        /// <param name="logMethod">This Action Delegate allows us to log to the scheduler console without any direct reference to it</param>
        /// <returns></returns>
        public StdTaskReturnValueBE Execute(string jobId, Action<LOG_LEVEL, string> logMethod)
        {
            logMethod.Invoke(LOG_LEVEL.INFO, $"Hello, custom info log message from inside plugin: [{nameof(AutoPayEventPublisher)}]!");
            logMethod.Invoke(LOG_LEVEL.WARNING, $"Hello, custom warning log message from inside plugin: [{nameof(AutoPayEventPublisher)}]!");
            logMethod.Invoke(LOG_LEVEL.ERROR, $"Hello, custom error log message from inside plugin: [{nameof(AutoPayEventPublisher)}]!");

            return new StdTaskReturnValueBE() { StepStatus = STD_STEP_STATUS.SUCCESS, ReturnMessage = "Ok" };
        }

        #region Implementation of Boilerplate Utility methods defined on Interface
        /// <summary>
        /// Returns Support info about this plugin
        /// </summary>
        /// <returns></returns>s>
        public string GetPlugInInfo()
        {
            var plugInAttribute = this.GetType().GetCustomAttribute<JobPlugInAttribute>();

            var compileTimestamp = this.GetType().Assembly
                                                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                                                    .First(a => a.Key == "CompileTimestamp")
                                                    .Value;

            return $"Name: [{plugInAttribute.Name}], Version: [{plugInAttribute.Version}], Compiled: [{compileTimestamp}]";
        }

        /// <summary>
        /// Injects the generic configuration known by the hosting process.
        /// </summary>
        /// <param name="kafkaConfig">The configuration information.</param>
        /// <remarks>
        /// This is called 1x after instantiation so that expensive initialization can occur
        /// </remarks>
        public void InjectConfig(KafkaServiceConfigBE kafkaConfig)
        {
            _kafkaServiceConfig = kafkaConfig;
        }

        #endregion
    }
}
