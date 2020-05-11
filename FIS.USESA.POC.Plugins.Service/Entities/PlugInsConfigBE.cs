using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FIS.USESA.POC.Plugins.Shared.Entities
{
    internal class PlugInsConfigBE
    {
        [JsonPropertyName(@"parentFolder")]
        public string PlugInsParentFolder { get; set; }

        [JsonPropertyName(@"assembliesToSkipLoading")]
        public List<string> AssembliesToSkipLoading { get; set; }
    }
}
