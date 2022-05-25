using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Issuelink
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public Type Type { get; set; }

        [JsonProperty("outwardIssue", NullValueHandling = NullValueHandling.Ignore)]
        public OutwardIssue OutwardIssue { get; set; }

        [JsonProperty("inwardIssue", NullValueHandling = NullValueHandling.Ignore)]
        public InwardIssue InwardIssue { get; set; }
    }
}
