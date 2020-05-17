using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FIS.USAESA.Autopay.Shared.Entities
{
    /// <summary>
    /// This class exists to confirm that assys referenced by the main plugin are successfully loaded
    /// </summary>
    public class AutopayEventBE
    {
        [JsonPropertyName(@"sechedulerJobId")]
        public string SchedulerJobId { get; set; }

        [JsonPropertyName(@"eventDtUtc")]
        public DateTime EventDTUTC { get; set; }

        [JsonPropertyName(@"counter")]
        public int Counter { get; set; }
    }
}
