using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Assignee
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatarUrls")]
        public AvatarUrls AvatarUrls { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        public string TimeZone { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
