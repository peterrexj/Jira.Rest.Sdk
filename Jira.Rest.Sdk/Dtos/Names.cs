using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Names
    {
        [JsonProperty("watcher", NullValueHandling = NullValueHandling.Ignore)]
        public string Watcher { get; set; }

        [JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
        public string Attachment { get; set; }

        [JsonProperty("sub-tasks", NullValueHandling = NullValueHandling.Ignore)]
        public string SubTasks { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("project", NullValueHandling = NullValueHandling.Ignore)]
        public string Project { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty("issuelinks", NullValueHandling = NullValueHandling.Ignore)]
        public string Issuelinks { get; set; }

        [JsonProperty("worklog", NullValueHandling = NullValueHandling.Ignore)]
        public string Worklog { get; set; }

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated { get; set; }

        [JsonProperty("timetracking", NullValueHandling = NullValueHandling.Ignore)]
        public string Timetracking { get; set; }
    }
}
