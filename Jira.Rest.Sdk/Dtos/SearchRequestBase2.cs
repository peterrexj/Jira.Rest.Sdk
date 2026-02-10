using Newtonsoft.Json;
using System;

namespace Jira.Rest.Sdk.Dtos;

/// <summary>
/// Base class for search requests using Jira API v3 pagination with startAt.
/// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
/// </summary>
public abstract class SearchRequestBase2
{
    /// <summary>
    /// The index of the first item to return in the page of results (page offset). The base index is 0.
    /// </summary>
    [JsonProperty("startAt", NullValueHandling = NullValueHandling.Ignore)]
    public Int64? StartAt { get; set; }

    /// <summary>
    /// The maximum number of items to return per page.
    /// </summary>
    [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
    public Int64? MaxResults { get; set; }
}
