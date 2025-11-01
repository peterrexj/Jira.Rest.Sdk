using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class AttachmentsList
    {
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public int Total { get; set; }

        [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxResults { get; set; }

        [JsonProperty("startAt", NullValueHandling = NullValueHandling.Ignore)]
        public int StartAt { get; set; }

        [JsonProperty("isLast", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLast { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public List<Attachment> Values { get; set; }
    }
}