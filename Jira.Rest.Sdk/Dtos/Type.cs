using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Type
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("inward", NullValueHandling = NullValueHandling.Ignore)]
        public string Inward { get; set; }

        [JsonProperty("outward", NullValueHandling = NullValueHandling.Ignore)]
        public string Outward { get; set; }
    }
}
