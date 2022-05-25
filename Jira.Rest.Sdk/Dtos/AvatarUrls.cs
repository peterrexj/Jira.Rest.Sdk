using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class AvatarUrls
    {
        [JsonProperty("48x48", NullValueHandling = NullValueHandling.Ignore)]
        public string _48x48 { get; set; }

        [JsonProperty("24x24", NullValueHandling = NullValueHandling.Ignore)]
        public string _24x24 { get; set; }

        [JsonProperty("16x16", NullValueHandling = NullValueHandling.Ignore)]
        public string _16x16 { get; set; }

        [JsonProperty("32x32", NullValueHandling = NullValueHandling.Ignore)]
        public string _32x32 { get; set; }
    }
}
