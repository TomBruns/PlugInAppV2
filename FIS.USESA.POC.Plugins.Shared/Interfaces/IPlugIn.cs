using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FIS.USESA.POC.Plugins.Shared.Entities;
using static FIS.USESA.POC.Plugins.Shared.Constants.SchedulerConstants;
using FIS.USESA.POC.Plugins.Shared.Attributes;

namespace FIS.USESA.POC.Plugins.Shared.Interfaces
{
    /// <summary>
    /// Interface IPlugIn
    /// </summary>
    /// <remarks>
    /// Define the interface that each plug-in will implement, each will be implemented in a separate, independent assy    
    /// </remarks>
    public interface IPlugIn
    {
        void InjectConfig(KafkaServiceConfigBE kafkaConfig);

        StdTaskReturnValueBE Execute(string jobId, Action<LOG_LEVEL, string> logMethod);

        // cannot use default implementation of this method in the interface (this is feature of C# 8.0)
        // because we need to use reflection
        string GetPlugInInfo();
    }
}
