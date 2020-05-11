using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using FIS.USESA.POC.Plugins.Shared.Entities;

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

        StdTaskReturnValueBE Execute(string jobId);

        string GetPlugInInfo();
    }
}
