using Newtonsoft.Json;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueMetadataSchema
    {
        [JsonProperty("custom")]
        public string Custom { get; set; }

        [JsonProperty("customId")]
        public int CustomId { get; set; }

        [JsonProperty("items")]
        public string Items { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
