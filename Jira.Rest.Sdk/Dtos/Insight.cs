using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Insight
    {
        [JsonProperty("totalIssueCount")]
        public int TotalIssueCount { get; set; }

        [JsonProperty("lastIssueUpdateTime")]
        public DateTime LastIssueUpdateTime { get; set; }
    }
}
