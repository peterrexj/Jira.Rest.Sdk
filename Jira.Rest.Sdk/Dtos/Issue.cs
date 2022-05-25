using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Issue
    {
        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public string Expand { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        [JsonProperty("names", NullValueHandling = NullValueHandling.Ignore)]
        public Names Names { get; set; }
    }
}
