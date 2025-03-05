using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueSearchRequest : SearchRequestBase2
    {
        [JsonProperty("jql", NullValueHandling = NullValueHandling.Ignore)]
        public string Jql { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Fields { get; set; }

        [JsonProperty("fieldsByKeys", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FieldsByKeys { get; set; }

        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public string? Expand { get; set; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Properties { get; set; }

        [JsonProperty("reconcileIssues", NullValueHandling = NullValueHandling.Ignore)]
        public int[]? ReconcileIssues { get; set; }
    }
}
