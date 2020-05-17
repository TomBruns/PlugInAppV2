using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

using FIS.USESA.POC.Plugins.Shared.Interfaces;

namespace FIS.USESA.POC.Plugins.Service.Entities
{
    /// <summary>
    /// This class describes a loaded plugin
    /// </summary>
    public class PlugInBE
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"version")]
        public Version Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"loadedAtDT")]
        public DateTime LoadedAtDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"plugInInfo")]
        public string PlugInInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"plugInPath")]
        public string PlugInPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName(@"assemblyLoadContextName")]
        public string AssemblyLoadContextName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public IPlugIn PlugInImpl { get; set; }
    }
}
