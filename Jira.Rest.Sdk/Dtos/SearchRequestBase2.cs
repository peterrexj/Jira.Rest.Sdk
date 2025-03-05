using Newtonsoft.Json;
using System;

namespace Jira.Rest.Sdk.Dtos;

public abstract class SearchRequestBase2
{
    [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
    public Int64? MaxResults { get; set; }

    [JsonProperty("nextPageToken", NullValueHandling = NullValueHandling.Ignore)]
    public string? NextPageToken { get; set; }
}
