using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Worklog
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public Author Author { get; set; }

        [JsonProperty("updateAuthor", NullValueHandling = NullValueHandling.Ignore)]
        public UpdateAuthor UpdateAuthor { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Updated { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public Visibility Visibility { get; set; }

        [JsonProperty("started", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Started { get; set; }

        [JsonProperty("timeSpent", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeSpent { get; set; }

        [JsonProperty("timeSpentSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeSpentSeconds { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("issueId", NullValueHandling = NullValueHandling.Ignore)]
        public string IssueId { get; set; }
    }
}
