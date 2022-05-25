using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Scope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }
    }
}
