using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Author
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("avatarUrls", NullValueHandling = NullValueHandling.Ignore)]
        public AvatarUrls AvatarUrls { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("active", NullValueHandling = NullValueHandling.Ignore)]
        public bool Active { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("emailAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string EmailAddress { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZone { get; set; }

        [JsonProperty("accountId", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty("accountType", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountType { get; set; }
    }
}
