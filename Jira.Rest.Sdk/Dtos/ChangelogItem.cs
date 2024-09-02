using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class ChangelogItem
    {
        [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        [JsonProperty("fieldtype", NullValueHandling = NullValueHandling.Ignore)]
        public string Fieldtype { get; set; }

        [JsonProperty("fieldId", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldId { get; set; }

        [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
        public string From { get; set; }

        [JsonProperty("fromString", NullValueHandling = NullValueHandling.Ignore)]
        public string FromString { get; set; }

        [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
        public string To { get; set; }

        [JsonProperty("toString", NullValueHandling = NullValueHandling.Ignore)]
        public string ToStringJira { get; set; }
    }
}
