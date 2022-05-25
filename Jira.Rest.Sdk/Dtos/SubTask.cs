using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class SubTask
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public Type Type { get; set; }

        [JsonProperty("outwardIssue", NullValueHandling = NullValueHandling.Ignore)]
        public OutwardIssue OutwardIssue { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        public Uri Self { get; set; }
    }
}
