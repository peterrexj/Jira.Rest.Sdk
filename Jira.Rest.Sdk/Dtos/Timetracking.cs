using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Timetracking
    {
        [JsonProperty("originalEstimate", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalEstimate { get; set; }

        [JsonProperty("remainingEstimate", NullValueHandling = NullValueHandling.Ignore)]
        public string RemainingEstimate { get; set; }

        [JsonProperty("timeSpent", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeSpent { get; set; }

        [JsonProperty("originalEstimateSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public int OriginalEstimateSeconds { get; set; }

        [JsonProperty("remainingEstimateSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public int RemainingEstimateSeconds { get; set; }

        [JsonProperty("timeSpentSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeSpentSeconds { get; set; }
    }
}
