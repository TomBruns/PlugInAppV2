using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Loader;

using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Hangfire.Storage;

using FIS.USESA.POC.Plugins.Shared.Entities;
using FIS.USESA.POC.Plugins.Service.PlugInSupport;
using FIS.USESA.POC.Plugins.Shared.Interfaces;
using FIS.USESA.POC.Plugins.Service.Logging;

using static FIS.USESA.POC.Plugins.Shared.Constants.SchedulerConstants;

namespace FIS.USESA.POC.Plugins.Service.Hangfire
{
    /// <summary>
    /// This class abstracts the interaction with the Job Scheduler Hangfire
    /// </summary>
    public class HangfireManager
    {
        private KafkaServiceConfigBE _kafkaConfig;
        //private ILogger _logger;
        //private IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> _jobPlugIns;
        private PlugInsManager _plugInsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireManager"/> class.
        /// </summary>
        /// <param name="plugInsManager">The PlugIn Manager.</param>
        /// <param name="kafkaConfig">The kafka configuration.</param>
        public HangfireManager(PlugInsManager plugInsManager, KafkaServiceConfigBE kafkaConfig)
        {
            _plugInsManager = plugInsManager;
            _kafkaConfig = kafkaConfig;
        }

        /// <summary>
        /// Enqueue a request with the information necessary to dynamically load the necessary assy on the other side of the hangfire queue
        /// </summary>
        [DisplayName("Queue Job Id: {0}, Name: {1} v{2}")]
        public static void EnqueueRequest(string jobId, string plugInName, decimal plugInVersion)
        {
            // the process that is submitting creates new fire & forget jobs
            // they can be processed in parallel on any available thread on any server running hangfire
            //BackgroundJob.Enqueue(() => RequestController.ExecuteRequest(className, assyName, methodName, parmValue));
            BackgroundJob.Enqueue<HangfireManager>(rc => rc.ExecuteRequest(jobId, plugInName, plugInVersion, null));
        }

        #region Load via Reflection Approach
        //// <summary>
        //// Dynamically load the correct assy and invoke the target method using reflection
        //// </summary>
        //[DisplayName("Execute Job, Plugin Class {0} => Method {2}]")]
        //public void ExecuteRequest(string className, string assyName, string methodName, string parmValue)
        //{
        //    // use reflection to dynamically execute the plugin method
        //    Type taskType = Type.GetType($"{className}, {assyName}");
        //    MethodInfo taskMethod = taskType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

        //    // NOTE: Normal Reflection Parameter Widening applies (ex: Int32 => Int64), so normal int target method params should be changed to long
        //    object[] parms = new object[] { parmValue };
        //    var returnValue = (StdTaskReturnValueBE)taskMethod.Invoke(null, parms);
        //}
        #endregion

        /// <summary>
        /// Executes the request using an implementation in a plug-in assy
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="pluginName">The plugin name.</param>
        /// <param name="plugInVersion">The plugin version.</param>
        /// <param name="context">The context.</param>
        [DisplayName("Execute Job Id: {0}, Name: {1} v{2}")]
        public void ExecuteRequest(string jobId, string pluginName, decimal plugInVersion, PerformContext context)
        {
            // create a logger
            var logger = context.CreateLoggerForPerformContext<HangfireManager>();

            // This is a 1st pass at preventing duplicate recurring job when the previous execution is still running
            var job = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => j.Id == jobId).FirstOrDefault();
            if (job != null)
            {
                if (job.LastJobState == "Enqueued" || job.LastJobState == "Processing")
                {
                    logger.Information("This goes to the job console automatically");

                    logger.Warning("Skipping execution of JobId: {jobId}, it is still running from a previous execution.", jobId);
                    return;
                }
            }

            // dynamically select the correct plug-in assy to use to process the event
            IPlugIn jobPlugIn = _plugInsManager.GetJobPlugIn(pluginName, plugInVersion).PlugInImpl;

            var plugInLoadContextName = AssemblyLoadContext.GetLoadContext(jobPlugIn.GetType().Assembly).Name;
            logger.Information("Running plugin {pluginInfo} in ALC: {plugInLoadContextName}.", jobPlugIn.GetPlugInInfo(), plugInLoadContextName);

            try
            {
                // call the method on the dynamically selected assy passing in an anonymous action delagate for the logging method
                var result = jobPlugIn.Execute(context.BackgroundJob.Id, (LOG_LEVEL logLevel, string logMessage) =>
                {
                    switch (logLevel)
                    {
                        case LOG_LEVEL.INFO:
                            logger.Information(logMessage);
                            break;

                        case LOG_LEVEL.WARNING:
                            logger.Warning(logMessage);
                            break;

                        case LOG_LEVEL.ERROR:
                            logger.Error(logMessage);
                            break;
                    }
                });

                // write post execution log message
                switch (result.StepStatus)
                {
                    case STD_STEP_STATUS.SUCCESS:
                        logger.Information(result.ReturnMessage);
                        break;
                    case STD_STEP_STATUS.FAILURE:
                        logger.Error(result.ReturnMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Unhandled exception occured in plugin: [{ex.ToString()}]");
            }
        }
    }
}
