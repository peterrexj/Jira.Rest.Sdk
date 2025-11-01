using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class BulkOperationResult
    {
        [JsonProperty("issues", NullValueHandling = NullValueHandling.Ignore)]
        public List<Issue> Issues { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public List<BulkOperationError> Errors { get; set; }
    }

    public class BulkOperationError
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }

        [JsonProperty("elementErrors", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> ElementErrors { get; set; }

        [JsonProperty("failedElementNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int FailedElementNumber { get; set; }
    }
}