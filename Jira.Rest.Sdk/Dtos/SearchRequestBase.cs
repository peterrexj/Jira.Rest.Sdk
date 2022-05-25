using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class SearchRequestBase
    {
        [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? maxResults { get; set; }

        [JsonProperty("startAt", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? startAt { get; set; }
    }
}
