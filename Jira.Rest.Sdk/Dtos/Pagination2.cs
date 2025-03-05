using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira.Rest.Sdk.Dtos
{
    public class Pagination2<T>
    {
        [JsonProperty("nextPageToken")]
        public string nextPageToken { get; set; }

        [JsonProperty("issues")]
        public List<T> issues { get; set; }

        [JsonProperty("values")]
        public List<T> values { get; set; }

        public List<T> PaginatedItems => issues ?? values;
    }
}
