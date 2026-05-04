using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Changelog
    {
        [JsonProperty("isLast", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLast { get; set; }

        [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxResults { get; set; }

        [JsonProperty("nextPage", NullValueHandling = NullValueHandling.Ignore)]
        public string NextPage { get; set; }

        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("startAt", NullValueHandling = NullValueHandling.Ignore)]
        public int StartAt { get; set; }

        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public int Total { get; set; }

        // dedicated /changelog endpoint response
        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChangelogEntry> Values { get; set; }

        // expand=changelog embedded in issue search response
        [JsonProperty("histories", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChangelogEntry> Histories { get; set; }

        // unified access regardless of source
        public List<ChangelogEntry> Entries => Histories ?? Values;

        // embedded response has no isLast — complete when entry count matches total
        public bool IsComplete => Entries != null && Entries.Count >= Total;
    }
}
