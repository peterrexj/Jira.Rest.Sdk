using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    /// <summary>
    /// Request object for the new Jira API v3 /search/jql endpoint.
    /// Uses nextPageToken for pagination instead of startAt.
    /// Reference: https://developer.atlassian.com/changelog/#CHANGE-2046
    /// </summary>
    public class IssueSearchRequest
    {
        [JsonProperty("jql", NullValueHandling = NullValueHandling.Ignore)]
        public string Jql { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Fields { get; set; }

        [JsonProperty("fieldsByKeys", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FieldsByKeys { get; set; }

        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public string? Expand { get; set; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Properties { get; set; }

        [JsonProperty("reconcileIssues", NullValueHandling = NullValueHandling.Ignore)]
        public int[]? ReconcileIssues { get; set; }

        /// <summary>
        /// The maximum number of items to return per page.
        /// </summary>
        [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxResults { get; set; }

        /// <summary>
        /// The token for the next page of results. Use the value from the previous response's nextPageToken.
        /// </summary>
        [JsonProperty("nextPageToken", NullValueHandling = NullValueHandling.Ignore)]
        public string? NextPageToken { get; set; }
    }
}
