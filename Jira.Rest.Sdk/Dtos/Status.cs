using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Status
    {
        [JsonProperty("iconUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        public Uri Self { get; set; }
        public string Description { get; set; }
        public long Id { get; set; }
        public StatusCategory StatusCategory { get; set; }
    }
}
