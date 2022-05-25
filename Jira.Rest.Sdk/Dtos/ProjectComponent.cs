using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class ProjectComponent
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("lead")]
        public Lead Lead { get; set; }

        [JsonProperty("assigneeType")]
        public string AssigneeType { get; set; }

        [JsonProperty("assignee")]
        public Assignee Assignee { get; set; }

        [JsonProperty("realAssigneeType")]
        public string RealAssigneeType { get; set; }

        [JsonProperty("realAssignee")]
        public RealAssignee RealAssignee { get; set; }

        [JsonProperty("isAssigneeTypeValid")]
        public bool IsAssigneeTypeValid { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("projectId")]
        public int ProjectId { get; set; }
    }
}
