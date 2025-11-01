using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueTransition
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
        public List<Status> From { get; set; }

        [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
        public Status To { get; set; }

        [JsonProperty("isGlobal", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsGlobal { get; set; }

        [JsonProperty("isInitial", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsInitial { get; set; }

        [JsonProperty("isAvailable", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsAvailable { get; set; }

        [JsonProperty("isConditional", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsConditional { get; set; }

        [JsonProperty("hasScreen", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasScreen { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Fields { get; set; }
    }
}