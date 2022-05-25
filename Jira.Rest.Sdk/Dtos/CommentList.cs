using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class CommentList
    {
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long Total { get; set; }

        [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
        public long MaxResults { get; set; }

        [JsonProperty("startAt", NullValueHandling = NullValueHandling.Ignore)]
        public long StartAt { get; set; }

        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Comment> Comments { get; set; }
    }
}
