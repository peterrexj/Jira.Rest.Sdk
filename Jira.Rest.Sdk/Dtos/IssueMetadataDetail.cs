using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueMetadataDetail
    {
        [JsonProperty("allowedValues")]
        public List<dynamic> AllowedValues { get; set; }

        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty("hasDefaultValue")]
        public bool HasDefaultValue { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("operations")]
        public List<string> Operations { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("schema")]
        public IssueMetadataSchema Schema { get; set; }
    }
}
