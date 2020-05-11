using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FIS.USESA.POC.Plugins.Shared.Entities
{
    /// <summary>
    /// This class describes the Kafka Config that can be passed into a plug-in
    /// </summary>
    public class KafkaServiceConfigBE
    {
        [JsonPropertyName(@"bootstrapServers")]
        public string BootstrapServers { get; set; }

        [JsonPropertyName(@"schemaRegistry")]
        public string SchemaRegistry { get; set; }
    }
}
