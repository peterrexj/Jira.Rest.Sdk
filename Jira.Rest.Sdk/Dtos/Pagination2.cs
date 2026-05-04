using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Jira.Rest.Sdk.Dtos
{
    /// <summary>
    /// Pagination response for Jira API v3 endpoints that use nextPage token-based pagination.
    /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
    /// </summary>
    public class Pagination2<T>
    {
        /// <summary>
        /// Whether this is the last page.
        /// </summary>
        [JsonProperty("isLast")]
        public bool IsLast { get; set; }

        /// <summary>
        /// The maximum number of results that could be on the page.
        /// </summary>
        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }

        /// <summary>
        /// The URL of the next page of results, if any. Used by project search and older endpoints.
        /// </summary>
        [JsonProperty("nextPage")]
        public string NextPage { get; set; }

        /// <summary>
        /// The opaque next-page cursor returned by the JQL issue search endpoint (/search/jql).
        /// </summary>
        [JsonProperty("nextPageToken")]
        public string NextPageTokenRaw { get; set; }

        /// <summary>
        /// The URL of the page.
        /// </summary>
        [JsonProperty("self")]
        public string Self { get; set; }

        /// <summary>
        /// The index of the first item returned on the page.
        /// </summary>
        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        /// <summary>
        /// The total number of results available.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// The list of items (for issue search, this will be "issues"; for project search, this will be "values").
        /// </summary>
        [JsonProperty("issues")]
        public List<T> Issues { get; set; }

        /// <summary>
        /// The list of items (for project search and other endpoints).
        /// </summary>
        [JsonProperty("values")]
        public List<T> Values { get; set; }

        /// <summary>
        /// Gets the paginated items from either issues or values list.
        /// </summary>
        public List<T> PaginatedItems => Issues ?? Values;

        /// <summary>
        /// Gets the next page token from either the JQL cursor (nextPageToken) or extracted from the NextPage URL (startAt param).
        /// Returns null if there is no next page.
        /// </summary>
        public string NextPageToken
        {
            get
            {
                if (!string.IsNullOrEmpty(NextPageTokenRaw))
                    return NextPageTokenRaw;

                if (string.IsNullOrEmpty(NextPage))
                    return null;

                var uri = new System.Uri(NextPage);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var values = query.GetValues("startAt");
                return values?.LastOrDefault();
            }
        }
    }
}
