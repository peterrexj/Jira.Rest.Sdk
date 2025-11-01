using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public partial class Votes
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Self { get; set; }

        [JsonProperty("votes", NullValueHandling = NullValueHandling.Ignore)]
        public long VotesCount { get; set; }

        [JsonProperty("hasVoted", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasVoted { get; set; }
    }
}
