using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Roles
    {
        [JsonProperty("Developers")]
        public string Developers { get; set; }
    }
}
