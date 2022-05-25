using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Version
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("released")]
        public bool Released { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }
    }
}
