using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FIS.USESA.POC.Plugins.Shared.Entities
{
    public class KafkaPluginConfigBE
    {
        [JsonPropertyName(@"kafkaTopicName")]
        public string KafkaTopicName { get; set; }
    }
}
