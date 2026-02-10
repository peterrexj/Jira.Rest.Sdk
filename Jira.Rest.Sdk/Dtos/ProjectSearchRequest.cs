using Newtonsoft.Json;

namespace Jira.Rest.Sdk.Dtos;

/// <summary>
/// Request model for searching projects using the paginated project search API.
/// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
/// </summary>
public class ProjectSearchRequest : SearchRequestBase2
{
    /// <summary>
    /// Filter the results using a literal string. Projects with a matching key or name are returned (case insensitive).
    /// </summary>
    [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
    public string? Query { get; set; }

    /// <summary>
    /// Order the results by a field.
    /// Valid values: category, issueCount, key, lastIssueUpdatedTime, name, owner, archivedDate, deletedDate
    /// Prefix with - for descending order (e.g., -name)
    /// </summary>
    [JsonProperty("orderBy", NullValueHandling = NullValueHandling.Ignore)]
    public string? OrderBy { get; set; }

    /// <summary>
    /// The project IDs to filter the results by. To include multiple IDs, provide an ampersand-separated list.
    /// For example, id=10000&id=10001. Up to 50 project IDs can be provided.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long[]? Id { get; set; }

    /// <summary>
    /// The project keys to filter the results by. To include multiple keys, provide an ampersand-separated list.
    /// For example, keys=PA&keys=PB. Up to 50 project keys can be provided.
    /// </summary>
    [JsonProperty("keys", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Keys { get; set; }

    /// <summary>
    /// Filter the results by project type.
    /// Valid values: business, service_desk, software
    /// </summary>
    [JsonProperty("typeKey", NullValueHandling = NullValueHandling.Ignore)]
    public string? TypeKey { get; set; }

    /// <summary>
    /// The ID of the project's category. A complete list of category IDs is found using Get all project categories.
    /// </summary>
    [JsonProperty("categoryId", NullValueHandling = NullValueHandling.Ignore)]
    public long? CategoryId { get; set; }

    /// <summary>
    /// Filter results by projects for which the user can perform the specified action.
    /// Valid values: view, browse, edit, create
    /// </summary>
    [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
    public string? Action { get; set; }

    /// <summary>
    /// Use expand to include additional information in the response. This parameter accepts a comma-separated list.
    /// Valid values: description, projectKeys, lead, issueTypes, url, insight
    /// </summary>
    [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
    public string? Expand { get; set; }

    /// <summary>
    /// Filter the results by project status.
    /// Valid values: live, archived, deleted
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Status { get; set; }

    /// <summary>
    /// A list of project properties to return for the project. This parameter accepts a comma-separated list.
    /// </summary>
    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Properties { get; set; }

    /// <summary>
    /// A query string used to filter projects by property. The query string cannot be specified with the properties parameter.
    /// </summary>
    [JsonProperty("propertyQuery", NullValueHandling = NullValueHandling.Ignore)]
    public string? PropertyQuery { get; set; }
}
