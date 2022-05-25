using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Pagination<T>
    {
        [JsonProperty("expand")]
        public string expand { get; set; }

        [JsonProperty("total")]
        public long total { get; set; }

        [JsonProperty("maxResults")]
        public long maxResults { get; set; }

        [JsonProperty("startAt")]
        public long startAt { get; set; }

        [JsonProperty("issues")]
        public List<T> issues { get; set; }

        [JsonProperty("values")]
        public List<T> values { get; set; }

        public List<T> PaginatedItems => issues ?? values;
    }
}
