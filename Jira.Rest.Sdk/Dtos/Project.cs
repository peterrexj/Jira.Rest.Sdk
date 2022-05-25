using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Project
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("avatarUrls", NullValueHandling = NullValueHandling.Ignore)]
        public AvatarUrls AvatarUrls { get; set; }

        [JsonProperty("projectCategory", NullValueHandling = NullValueHandling.Ignore)]
        public ProjectCategory ProjectCategory { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("lead", NullValueHandling = NullValueHandling.Ignore)]
        public Lead Lead { get; set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public List<ProjectComponent> Components { get; set; }

        [JsonProperty("issueTypes", NullValueHandling = NullValueHandling.Ignore)]
        public List<IssueType> IssueTypes { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("assigneeType", NullValueHandling = NullValueHandling.Ignore)]
        public string AssigneeType { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Versions { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public Roles Roles { get; set; }

        [JsonProperty("simplified", NullValueHandling = NullValueHandling.Ignore)]
        public bool Simplified { get; set; }

        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public Properties Properties { get; set; }

        [JsonProperty("insight", NullValueHandling = NullValueHandling.Ignore)]
        public Insight Insight { get; set; }
    }
}
