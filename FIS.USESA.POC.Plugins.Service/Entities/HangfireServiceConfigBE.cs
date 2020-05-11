using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FIS.USESA.POC.Plugins.Shared.Entities
{
    internal class HangfireServiceConfigBE
    {
        [JsonPropertyName(@"isUseSSL")]
        public bool IsUseSSL { get; set; }

        [JsonPropertyName(@"dashboardPortNumber")]
        public int DashboardPortNumber { get; set; }

        [JsonPropertyName(@"isDashboardRemoteAccessEnabled")]
        public bool IsDashboardRemoteAccessEnabled { get; set; }

        [JsonPropertyName(@"workerCount")]
        public int WorkerCount { get; set; }

        [JsonPropertyName(@"pollIntervalInSecs")]
        public int PollIntervalInSecs { get; set; }
    }
}
