using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueSearchRequest : SearchRequestBase
    {
        [JsonProperty("jql", NullValueHandling = NullValueHandling.Ignore)]
        public string jql { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public string[] fields { get; set; }

        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public string expand { get; set; }
    }
}
