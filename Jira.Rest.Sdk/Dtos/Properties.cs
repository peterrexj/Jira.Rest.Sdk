using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Properties
    {
        [JsonProperty("propertyKey")]
        public string PropertyKey { get; set; }
    }
}
