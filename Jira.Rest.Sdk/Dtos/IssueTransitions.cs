using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueTransitions
    {
        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public string Expand { get; set; }

        [JsonProperty("transitions", NullValueHandling = NullValueHandling.Ignore)]
        public List<IssueTransition> Transitions { get; set; }
    }
}