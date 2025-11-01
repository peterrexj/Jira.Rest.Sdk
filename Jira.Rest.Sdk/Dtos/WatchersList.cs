using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class WatchersList
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("isWatching", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsWatching { get; set; }

        [JsonProperty("watchCount", NullValueHandling = NullValueHandling.Ignore)]
        public int WatchCount { get; set; }

        [JsonProperty("watchers", NullValueHandling = NullValueHandling.Ignore)]
        public List<Watcher> Watchers { get; set; }
    }
}