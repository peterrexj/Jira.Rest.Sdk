using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueType
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtask")]
        public bool Subtask { get; set; }

        [JsonProperty("avatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("hierarchyLevel")]
        public int HierarchyLevel { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("scope")]
        public Scope Scope { get; set; }
    }
}
