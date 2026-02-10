using Jira.Rest.Sdk.Dtos;
using Newtonsoft.Json.Linq;
using Pj.Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TestAny.Essentials.Api;
using TestAny.Essentials.Core.Dtos.Api;
using Version = Jira.Rest.Sdk.Dtos.Version;

namespace Jira.Rest.Sdk
{
    public class JiraService : JiraServiceBase
    {
        /// <summary>
        /// Initialize the Zeyphr service with required parameters
        /// </summary>
        /// <param name="appUrl">Zeyphr endpoint</param>
        /// <param name="serviceUsername">Username for connecting to Zeyphr</param>
        /// <param name="servicePassword">Password for connecting to Zeyphr</param>
        /// <param name="jiraApiVersion">Jira Api version where the Zeyphr is hosted (default value: 2)</param>
        /// <param name="folderSeparator">Folder separator string (default value: '/')</param>
        /// <param name="logPrefix">Prefix text that will be added to all the logs generated from this service (default value: 'Zeyphr: ')</param>
        /// <param name="pageSizeSearchResult">Page size for search request (default value: '50')</param>
        /// <param name="requestRetryTimes">Number of time to retry when there is a network failure (default value: '1'). You can increase the number of times to retry based on your infrastructure if there are chance for a request to fail randomly</param>
        /// <param name="timeToSleepBetweenRetryInMilliseconds">Time to sleep in milliseconds between each time a call is retry (default value: '1000'). Applied only when requestRetryTimes is more than 1</param>
        /// <param name="assertResponseStatusOk">True/False whether the response code status from the server needs to be asserted for OK (default value 'true')</param>
        /// <param name="listOfResponseCodeOnFailureToRetry">Any of these status code matched from response will then use for retry the request. For example Proxy Authentication randomly failing can be then used to retry (default value 'null' which means it is not checking any response code for fail retry)</param>
        /// <param name="retryOnRequestTimeout">True/False whether the request should retry on when the server fails to respond within the timeout period, retry on when server timeouts for a request</param>
        /// <param name="proxyKeyName">Key to the proxy details. Refer readme for more information on how to set the custom proxy for every request</param>
        /// <param name="authToken">Authorization token to access the Jira Service</param>
        public JiraService(string appUrl,
            string serviceUsername,
            string servicePassword,
            bool isCloudVersion,
            string jiraApiVersion = "2",
            string folderSeparator = "/",
            string logPrefix = "Jira: ",
            int pageSizeSearchResult = 50,
            int requestRetryTimes = 1,
            int timeToSleepBetweenRetryInMilliseconds = 1000,
            bool assertResponseStatusOk = true,
            HttpStatusCode[] listOfResponseCodeOnFailureToRetry = null,
            int requestTimeoutInSeconds = 300,
            bool retryOnRequestTimeout = false,
            string proxyKeyName = "",
            string authToken = "")
                : base(appUrl, serviceUsername, servicePassword, isCloudVersion,
                      jiraApiVersion, folderSeparator, logPrefix, pageSizeSearchResult,
                      requestRetryTimes, timeToSleepBetweenRetryInMilliseconds, assertResponseStatusOk,
                      listOfResponseCodeOnFailureToRetry, requestTimeoutInSeconds, retryOnRequestTimeout,
                      proxyKeyName, authToken)
        { }

        #region Issues

        /// <summary>
        /// Creates a new issue in Jira. 
        /// Decription is removed as there defect in the Jira version 3 API
        /// </summary>
        /// <param name="projectKey">The key of the project in which to create the issue.</param>
        /// <param name="issueType">The type of the issue to create (e.g., Bug, Task).</param>
        /// <param name="summary">A brief summary of the issue.</param>
        /// <param name="priority">The priority of the issue (e.g., High, Medium, Low).</param>
        /// <param name="parentKey">The key of the parent issue, if any. Defaults to null.</param>
        /// <returns>The created issue.</returns>
        public Issue IssueCreate(string projectKey, string issueType,
            string summary,
            //Description description,
            string priority,
            string parentKey = null)
        {
            var projectMetadata = ProjectMetaInfoCache(projectKey);

            var issueId = projectMetadata.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueType}')].id").FirstOrDefault();
            var priorityId = projectMetadata.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueType}')].fields.priority.allowedValues[?(@.name== '{priority}')].id").FirstOrDefault();
            var priorityIds = projectMetadata.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueType}')]");

            if (issueId.IsEmpty()) throw new Exception($"The issue type requested to create [{issueType}] is not available!");
            if (priorityId.IsEmpty()) throw new Exception($"The priority requested to create [{priorityId}] is not available!");

            var createIssueRequestModel = new CreateIssueRequest
            {
                Fields = new Fields
                {
                    Project = new Project
                    {
                        Key = projectKey
                    },
                    Summary = summary,
                    //Description = description, TODO: Add description based on the version 3, currently its failing on Jira side with the object and no proper documentation of the error
                    IssueType = new IssueType
                    {
                        Id = issueId.ToLong()
                    }
                }
            };

            if (parentKey.HasValue())
            {
                createIssueRequestModel.Fields.Parent = new Project { Key = parentKey };
            }

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue")
                 .SetJsonBody(createIssueRequestModel)
                 .SetTimeout(RequestTimeoutInSeconds)
                 .PostWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.Created)
                throw new Exception("The Jira Issue creation failed!");

            return ToType<Issue>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves the approximate count of issues in Jira using JQL.
        /// </summary>
        /// <param name="jql">The JQL query string to use for searching issues.</param>
        /// <returns>The approximate count of issues that match the JQL query.</returns>
        public int IssueSearchApproximateCount(string jql)
        {
            var requestBody = new { jql = jql };
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/search/approximate-count")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            var responseContent = jiraResponse.ResponseBody.ContentString;
            var approximateCount = JObject.Parse(responseContent)["count"].Value<int>();
            return approximateCount;
        }

        /// <summary>
        /// Creates a link between two issues in Jira.
        /// </summary>
        /// <param name="linkType">The type of link to create (e.g., "blocks", "relates to").</param>
        /// <param name="outwardIssueKey">The key of the outward issue.</param>
        /// <param name="inwardIssueKey">The key of the inward issue.</param>
        /// <param name="issueInfo">Optional additional information about the issue link. Defaults to null.</param>
        /// <param name="comment">Optional comment to add to the issue link. Defaults to null.</param>
        public void IssueLink(string linkType, string outwardIssueKey, string inwardIssueKey, Issue? issueInfo = null, object? comment = null)
        {
            if (linkType.IsEmpty() || inwardIssueKey.IsEmpty() || outwardIssueKey.IsEmpty()) return;

            var metaIssueLinkTypes = IssueLinkTypesMetadataGet();
            var issueLinkId = metaIssueLinkTypes.ResponseBody.FilterJsonContent($"$.issueLinkTypes[*].name").FirstOrDefault(a => a.EqualsIgnoreCase(linkType));

            if (issueLinkId.IsEmpty()) return;

            if (outwardIssueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(outwardIssueKey);
            }

            if (issueInfo != null)
            {
                if (!issueInfo.Fields.Issuelinks.Any(a => a.InwardIssue.Key.EqualsIgnoreCase(inwardIssueKey)))
                {
                    var requestModel = new CreateIssueLink
                    {
                        Type = new Dtos.Type { Name = issueLinkId },
                        InwardIssue = new InwardIssue { Key = inwardIssueKey },
                        OutwardIssue = new OutwardIssue { Key = outwardIssueKey },
                        Comment = comment
                    };

                    var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issueLink")
                        .SetJsonBody(requestModel)
                        .SetTimeout(RequestTimeoutInSeconds)
                        .PostWithRetry(assertOk: AssertResponseStatusOk,
                           timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                           retryOption: RequestRetryTimes,
                           httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                           retryOnRequestTimeout: RetryOnRequestTimeout);

                    if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.Created)
                        throw new Exception("The link was not created!");
                }
            }
        }

        /// <summary>
        /// Retrieves an issue link from Jira by its ID.
        /// </summary>
        /// <param name="linkId">The ID of the issue link to retrieve.</param>
        /// <returns>The issue link corresponding to the specified ID.</returns>
        public IssueLink IssueLinkGetById(string linkId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issueLink/{linkId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<IssueLink>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Adds labels to an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to which labels will be added.</param>
        /// <param name="appendToExisting">Indicates whether to append the new labels to the existing ones. If false, existing labels will be replaced.</param>
        /// <param name="jiraIssueRead">Optional issue information. Defaults to null.</param>
        /// <param name="labels">The labels to add to the issue.</param>
        public void IssueLabelAdd(string issueKey, bool appendToExisting, Issue jiraIssueRead = null, params string[] labels)
        {
            var updateListOfLabels = labels.ToList();
            if (appendToExisting)
            {
                if (jiraIssueRead == null)
                {
                    jiraIssueRead = IssueGetById(issueKey);
                }
                if (updateListOfLabels.Except(jiraIssueRead.Fields.Labels).IsEmpty()) return;
                updateListOfLabels.AddRange(jiraIssueRead.Fields.Labels);
                updateListOfLabels = updateListOfLabels.Distinct().ToList();
            }

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.Labels = new List<OperationJiraPropertySet>();
            reqBody.Update.Labels.Add(new OperationJiraPropertySet { Set = updateListOfLabels.ToArray() });

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
              .SetJsonBody(reqBody)
              .WithJsonResponse()
              .SetTimeout(RequestTimeoutInSeconds)
              .PutWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }

        /// <summary>
        /// Assigns an issue to a user by their account ID in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to be assigned.</param>
        /// <param name="userId">The account ID of the user to whom the issue will be assigned.</param>
        public void IssueAssigneeByAccountId(string issueKey, string userId)
        {
            var reqBody = new { accountId = userId };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/assignee")
               .SetJsonBody(reqBody)
               .SetTimeout(RequestTimeoutInSeconds)
               .PutWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The issue assignment was not properly performed!");
        }

        /// <summary>
        /// Updates the components of an issue in Jira.
        /// </summary>
        /// <param name="components">A list of components to update.</param>
        /// <param name="appendMode">Indicates whether to append the new components to the existing ones. If false, existing components will be replaced.</param>
        /// <param name="issueKey">The key of the issue to update. Defaults to null.</param>
        /// <param name="issueInfo">Optional issue information. Defaults to null.</param>
        public void IssueComponentUpdate(List<string> components, bool appendMode, string issueKey = null, Issue issueInfo = null)
        {
            if (issueKey.IsEmpty() && issueInfo == null) return;

            if (issueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(issueKey);
            }

            var responseOnMetadataOfIssue = ProjectMetaInfoCache(issueInfo.Fields.Project.Key);

            foreach (var component in components)
            {
                var componentId = responseOnMetadataOfIssue.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueInfo.Fields.IssueType.Name}')].fields.components.allowedValues[?(@.name== '{component}')].id").FirstOrDefault();
                if (componentId.IsEmpty()) throw new Exception($"The component [{component}] is not availabe in the project [{issueInfo.Fields.Project.Key}] for the issue type of [{issueInfo.Fields.IssueType.Name}].");
            }

            if (appendMode)
            {
                if (components.IsEmpty()) return;
                if (components.Except(issueInfo.Fields?.Components?.Select(s => s.Name).DefaultIfEmpty()).IsEmpty()) return;
            }

            var reqBody = new UpdateIssueRequest { Update = new Update { Components = new List<Component> { } } };
            if (appendMode)
            {
                reqBody.Update.Components.AddRange(components.Select(s => new Component { Add = new OperationToIssueField { Name = s } }));
            }
            else
            {
                reqBody.Update.Components.AddRange(components.Select(s => new Component { Set = new OperationToIssueField { Name = s } }));
            }

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
              .SetJsonBody(reqBody)
              .WithJsonResponse()
              .SetTimeout(RequestTimeoutInSeconds)
              .PutWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }

        /// <summary>
        /// Updates the affected versions of an issue in Jira.
        /// </summary>
        /// <param name="affectedVersion">A list of affected versions to update.</param>
        /// <param name="appendMode">Indicates whether to append the new versions to the existing ones. If false, existing versions will be replaced.</param>
        /// <param name="issueKey">The key of the issue to update. Defaults to null.</param>
        /// <param name="issueInfo">Optional issue information. Defaults to null.</param>
        public void IssueAffectedVersionUpdate(List<string> affectedVersion, bool appendMode, string issueKey = null, Issue issueInfo = null)
        {
            if (issueKey.IsEmpty() && issueInfo == null) return;
            if (affectedVersion.IsEmpty()) return;

            if (issueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(issueKey);
            }

            var responseOnMetadataOfIssue = ProjectMetaInfoCache(issueInfo.Fields.Project.Key);

            foreach (var version in affectedVersion)
            {
                var componentId = responseOnMetadataOfIssue.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueInfo.Fields.IssueType.Name}')].fields.versions.allowedValues[?(@.name== '{version}')].id").FirstOrDefault();
                if (componentId.IsEmpty()) throw new Exception($"The component [{version}] is not availabe in the project [{issueInfo.Fields.Project.Key}] for the issue type of [{issueInfo.Fields.IssueType.Name}].");
            }

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.AffectedVersions = new List<dynamic>();
            if (appendMode)
            {
                reqBody.Update.AffectedVersions = new List<dynamic>();
                if (issueInfo.Fields.Versions.Any())
                {
                    reqBody.Update.AffectedVersions.AddRange(issueInfo.Fields.Versions.Select(s => new Component { Remove = new OperationToIssueField { Name = s.Name } }).ToList());
                }
            }
            reqBody.Update.AffectedVersions.AddRange(affectedVersion.Select(s => new Component { Add = new OperationToIssueField { Name = s } }).ToList());

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                  .SetJsonBody(reqBody)
                  .WithJsonResponse()
                  .SetTimeout(RequestTimeoutInSeconds)
                  .PutWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }

        /// <summary>
        /// Removes the affected versions from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue from which to remove affected versions. Defaults to null.</param>
        /// <param name="issueInfo">Optional issue information. Defaults to null.</param>
        public void IssueAffectedVersionRemove(string issueKey = null, Issue issueInfo = null)
        {
            if (issueKey.IsEmpty() && issueInfo == null) return;

            if (issueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(issueKey);
            }
            if (issueInfo.Fields.FixVersion.IsEmpty()) return;

            var responseOnMetadataOfIssue = ProjectMetaInfoCache(issueInfo.Fields.Project.Key);

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.AffectedVersions = new List<dynamic>();
            reqBody.Update.AffectedVersions.AddRange(issueInfo.Fields.Versions.Select(s => new Component { Remove = new OperationToIssueField { Name = s.Name } }).ToList());

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                  .SetJsonBody(reqBody)
                  .WithJsonResponse()
                  .SetTimeout(RequestTimeoutInSeconds)
                  .PutWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }

        /// <summary>
        /// Updates the fix versions of an issue in Jira.
        /// </summary>
        /// <param name="fixVersion">A list of fix versions to update.</param>
        /// <param name="appendMode">Indicates whether to append the new versions to the existing ones. If false, existing versions will be replaced.</param>
        /// <param name="issueKey">The key of the issue to update. Defaults to null.</param>
        /// <param name="issueInfo">Optional issue information. Defaults to null.</param>
        public void IssueFixVersionUpdate(List<string> fixVersion, bool appendMode, string issueKey = null, Issue issueInfo = null)
        {
            if (issueKey.IsEmpty() && issueInfo == null) return;
            if (fixVersion.IsEmpty()) return;

            if (issueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(issueKey);
            }

            var responseOnMetadataOfIssue = ProjectMetaInfoCache(issueInfo.Fields.Project.Key);

            foreach (var version in fixVersion)
            {
                var componentId = responseOnMetadataOfIssue.ResponseBody.FilterJsonContent($"$.projects[*].issuetypes[?(@.name== '{issueInfo.Fields.IssueType.Name}')].fields.versions.allowedValues[?(@.name== '{version}')].id").FirstOrDefault();
                if (componentId.IsEmpty()) throw new Exception($"The component [{version}] is not availabe in the project [{issueInfo.Fields.Project.Key}] for the issue type of [{issueInfo.Fields.IssueType.Name}].");
            }

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.FixVersions = new List<dynamic>();
            if (appendMode)
            {
                reqBody.Update.FixVersions = new List<dynamic>();
                if (issueInfo.Fields.FixVersion.Any())
                {
                    reqBody.Update.FixVersions.AddRange(issueInfo.Fields.FixVersion.Select(s => new Component { Remove = new OperationToIssueField { Name = s.Name } }).ToList());
                }
            }
            reqBody.Update.FixVersions.AddRange(fixVersion.Select(s => new Component { Add = new OperationToIssueField { Name = s } }).ToList());

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                  .SetJsonBody(reqBody)
                  .WithJsonResponse()
                  .SetTimeout(RequestTimeoutInSeconds)
                  .PutWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The fix version was not updated!");
        }

        /// <summary>
        /// Removes the fix versions from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue from which to remove fix versions. Defaults to null.</param>
        /// <param name="issueInfo">Optional issue information. Defaults to null.</param>
        public void IssueFixVersionRemove(string issueKey = null, Issue issueInfo = null)
        {
            if (issueKey.IsEmpty() && issueInfo == null) return;

            if (issueKey.HasValue() && issueInfo == null)
            {
                issueInfo = IssueGetById(issueKey);
            }
            if (issueInfo.Fields.FixVersion.IsEmpty()) return;

            var responseOnMetadataOfIssue = ProjectMetaInfoCache(issueInfo.Fields.Project.Key);

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.FixVersions = new List<dynamic>();
            reqBody.Update.FixVersions.AddRange(issueInfo.Fields.FixVersion.Select(s => new Component { Remove = new OperationToIssueField { Name = s.Name } }).ToList());

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                  .SetJsonBody(reqBody)
                  .WithJsonResponse()
                  .SetTimeout(RequestTimeoutInSeconds)
                  .PutWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The fix version was not updated!");
        }

        /// <summary>
        /// Updates the description of an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to update.</param>
        /// <param name="description">The new description for the issue.</param>
        public void IssueDescriptionUpdate(string issueKey, string description)
        {
            if (issueKey.IsEmpty()) return;

            // Format the description as Atlassian Document Format (ADF) for Jira Cloud/Server compatibility
            var formattedDescription = new
            {
                type = "doc",
                version = 1,
                content = new[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = description
                            }
                        }
                    }
                }
            };

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.Description = new List<dynamic>();
            reqBody.Update.Description.Add(new { set = formattedDescription });

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                  .SetJsonBody(reqBody)
                  .WithJsonResponse()
                  .SetTimeout(RequestTimeoutInSeconds)
                  .PutWithRetry(assertOk: AssertResponseStatusOk,
                       timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                       retryOption: RequestRetryTimes,
                       httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                       retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The description was not updated!");
        }

        /// <summary>
        /// Deletes an issue in Jira.
        /// </summary>
        /// <param name="issueIdOrKey">The ID or key of the issue to delete.</param>
        /// <param name="deleteSubtasks">Indicates whether to delete subtasks of the issue. Defaults to false.</param>
        public void IssueDelete(string issueIdOrKey, bool deleteSubtasks = false)
        {
            var jiraResponse = OpenRequest($" /rest/api/{JiraApiVersion}/issue/{issueIdOrKey}")
                .SetQueryParams(new ParameterCollection { { "deleteSubtasks", deleteSubtasks } })
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }

        /// <summary>
        /// Retrieves metadata for a specific issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve metadata.</param>
        /// <returns>A list of issue metadata.</returns>
        public List<IssueMetadata> IssueMetadataGet(string issueKey)
        {
            var response = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/editmeta")
               .SetTimeout(RequestTimeoutInSeconds)
               .GetWithRetry(assertOk: AssertResponseStatusOk,
                  timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                  retryOption: RequestRetryTimes,
                  httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                  retryOnRequestTimeout: RetryOnRequestTimeout);

            response.AssertResponseStatusForSuccess();

            List<IssueMetadata> result = new();
            var fieldsFromServer = response.ResponseBody.FilterJsonContent("$.fields.*");

            foreach (var field in fieldsFromServer)
            {
                try
                {
                    var data = SerializationHelper.DeSerializeJsonFromString<IssueMetadataDetail>(field);
                    result.Add(new IssueMetadata { Name = data.Name, Metadata = data });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing item: {ex.Message}");
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the changelogs for a specific issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve changelogs.</param>
        /// <returns>A list of changelog entries for the issue.</returns>
        public List<ChangelogEntry> IssueChangelogsGet(string issueKey)
        {
            return SearchFull<ChangelogEntry>(
                new { issueKey = issueKey }.GetPropertyValuesV2(),
                (s) => IssueChangelogsGet(s), predicate: null, breakSearchOnFirstConditionValid: false).ToList();
        }
        private Pagination<ChangelogEntry> IssueChangelogsGet(IDictionary<string, string> request)
        {
            var response = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{request["issueKey"]}/changelog")
                .SetQueryParams(request)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            response.AssertResponseStatusForSuccess();
            return ToType<Pagination<ChangelogEntry>>(response.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves an issue from Jira by its key.
        /// </summary>
        /// <param name="issueKey">The key of the issue to retrieve.</param>
        /// <param name="fields">A comma-separated list of fields to include in the response. Defaults to "*all" to include all fields.</param>
        /// <param name="extractDynamicFields">Indicates whether to extract dynamic fields from the issue. Defaults to false.</param>
        /// <param name="dynamicFieldsIncludeEmptyValues">Indicates whether to include empty values for dynamic fields. Defaults to true.</param>
        /// <returns>The issue corresponding to the specified key.</returns>
        public Issue IssueGetById(string issueKey, string fields = "*all",
            bool extractDynamicFields = false, bool dynamicFieldsIncludeEmptyValues = true)
        {
            Log($"Trying to get jira issue [{issueKey}]");

            ParameterCollection paramCollection = new();
            paramCollection.Add("fields", fields);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
                .SetQueryParams(paramCollection)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            if (extractDynamicFields)
            {
                var issue = ToType<Issue>(jiraResponse.ResponseBody.ContentString);
                issue.FieldsDynamic = new Dictionary<string, dynamic>();

                var jsonObject = JObject.Parse(jiraResponse.ResponseBody.ContentString);
                var fieldsInResponse = jsonObject.SelectToken("$.fields") as JObject;
                foreach (var field in fieldsInResponse)
                {
                    try
                    {
                        if (field.Value == null) continue;
                        if (dynamicFieldsIncludeEmptyValues)
                        {
                            issue.FieldsDynamic.Add(field.Key, field.Value);
                        }
                        else
                        {
                            var keyValuePair = new KeyValuePair<string, JToken>(field.Key, field.Value);
                            if (keyValuePair.Value != null && !IsEmptyToken(keyValuePair.Value))
                            {
                                issue.FieldsDynamic.Add(field.Key, field.Value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        Console.WriteLine($"Error deserializing item: {ex.Message}");
                    }
                }

                return issue;
            }
            else
            {
                return ToType<Issue>(jiraResponse.ResponseBody.ContentString);
            }
        }

        /// <summary>
        /// Internal method for executing a single page issue search request using the new /rest/api/3/search/jql endpoint.
        /// This method is called by IssueSearchWithPagination to retrieve each page of results.
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
        /// </summary>
        /// <param name="issueSearchRequest">The search request containing JQL query and pagination parameters.</param>
        /// <returns>A paginated result containing issues and the nextPageToken for subsequent requests.</returns>
        internal Pagination2<Issue> IssueSearch(IssueSearchRequest issueSearchRequest)
        {
            //Using POST to handle large query string
            // Note: Jira Cloud has migrated to /rest/api/3/search/jql endpoint
            // Reference: https://developer.atlassian.com/changelog/#CHANGE-2046
            // Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/search/jql")
                .SetJsonBody(issueSearchRequest)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Pagination2<Issue>>(jiraResponse.ResponseBody.ContentString);
        }

        //"TODO: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
        //"TODO: https://developer.atlassian.com/changelog/#CHANGE-2046

        /// <summary>
        /// Searches for issues in Jira using JQL (Jira Query Language).
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
        /// </summary>
        /// <param name="jql">The JQL query string to use for searching issues.</param>
        /// <param name="fields">A list of fields to return for each issue. By default, only IDs are returned by the new API.</param>
        /// <param name="fieldsByKeys">Whether the fields parameter accepts field IDs instead of field names. Default is false.</param>
        /// <param name="expand">A comma-separated list of the parameters to expand.</param>
        /// <param name="properties">A list of issue properties to return for each issue. By default, no properties are returned.</param>
        /// <param name="reconcileIssues">A list of issue IDs to reconcile. Default is null.</param>
        /// <param name="predicate">An optional predicate to filter the results. Defaults to null.</param>
        /// <param name="breakSearchOnFirstConditionValid">Indicates whether to stop the search when the first condition is met. Defaults to true.</param>
        /// <returns>A list of issues that match the JQL query and optional predicate.</returns>
        public List<Issue> IssueSearch(
            string jql,
            string[]? fields = null,
            bool fieldsByKeys = false,
            string? expand = null,
            string[]? properties = null,
            int[]? reconcileIssues = null,
            Func<Issue, bool>? predicate = null, bool breakSearchOnFirstConditionValid = true)
        {
            return IssueSearchWithPagination(
                new IssueSearchRequest
                {
                    Jql = jql,
                    Fields = fields,
                    FieldsByKeys = fieldsByKeys ? true : null, // Only set if true, otherwise null to be ignored
                    Expand = expand,
                    Properties = properties,
                    ReconcileIssues = reconcileIssues
                },
                predicate, breakSearchOnFirstConditionValid).ToList();
        }

        /// <summary>
        /// Internal method for paginated issue search using the new nextPageToken approach.
        /// </summary>
        private IList<Issue> IssueSearchWithPagination(
            IssueSearchRequest searchRequest,
            Func<Issue, bool>? predicate = null,
            bool breakSearchOnFirstConditionValid = true)
        {
            var results = new List<Issue>();
            if (searchRequest == null) return results;

            // Use provided MaxResults or default to 50
            if (!searchRequest.MaxResults.HasValue || searchRequest.MaxResults.Value <= 0)
            {
                searchRequest.MaxResults = 50;
            }

            do
            {
                var resp = IssueSearch(searchRequest);
                if (predicate != null)
                {
                    foreach (var value in resp.PaginatedItems)
                    {
                        if (predicate(value) == true)
                        {
                            results.Add(value);
                            if (breakSearchOnFirstConditionValid)
                            {
                                return results;
                            }
                        }
                    }
                }
                else
                {
                    resp.PaginatedItems.Iter(r => results.Add(r));
                }

                // Check if there are more pages using isLast or nextPageToken
                if (resp.IsLast || string.IsNullOrEmpty(resp.NextPageToken))
                {
                    break;
                }

                // Update nextPageToken for the next page
                searchRequest.NextPageToken = resp.NextPageToken;
            }
            while (true);

            return results;
        }


        /// <summary>
        /// Retrieves metadata for creating an issue in Jira for a specific project and optionally an issue type.
        /// </summary>
        /// <param name="project">The key of the project for which to retrieve issue creation metadata.</param>
        /// <param name="issueType">The type of the issue for which to retrieve metadata. Optional.</param>
        /// <returns>A TestApiResponse containing the issue creation metadata.</returns>
        public TestApiResponse IssueCreateMetaDataGet(string project, string issueType = null)
        {
            var qryParam = new ParameterCollection();
            qryParam.Add("projectKeys", project);
            if (issueType.HasValue())
            {
                qryParam.Add("issuetypeNames", issueType);
            }
            qryParam.Add("expand", "projects.issuetypes.fields");

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/createmeta")
               .SetQueryParams(qryParam)
               .SetTimeout(RequestTimeoutInSeconds)
               .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            return jiraResponse;
        }

        /// <summary>
        /// Retrieves metadata for the available issue link types in Jira.
        /// </summary>
        /// <returns>A TestApiResponse containing the issue link types metadata.</returns>
        public TestApiResponse IssueLinkTypesMetadataGet()
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issueLinkType")
               .SetTimeout(RequestTimeoutInSeconds)
               .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            return jiraResponse;
        }

        /// <summary>
        /// Deletes an issue link in Jira.
        /// </summary>
        /// <param name="linkId">The ID of the issue link to delete.</param>
        public void IssueLinkDelete(string linkId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issueLink/{linkId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The issue link was not deleted!");
        }

        /// <summary>
        /// Retrieves the available transitions for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve transitions.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The available transitions for the issue.</returns>
        public IssueTransitions IssueTransitionsGet(string issueKey, string expand = null)
        {
            var queryParams = new ParameterCollection();
            if (expand.HasValue())
            {
                queryParams.Add("expand", expand);
            }

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/transitions")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<IssueTransitions>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Transitions an issue to a new status in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to transition.</param>
        /// <param name="transitionId">The ID of the transition to perform.</param>
        /// <param name="comment">Optional comment to add during the transition.</param>
        /// <param name="fields">Optional fields to update during the transition.</param>
        public void IssueTransition(string issueKey, string transitionId, string comment = null, object fields = null)
        {
            object updateSection = null;
            if (comment.HasValue())
            {
                // Format the comment body as Atlassian Document Format (ADF) for Jira Cloud/Server compatibility
                var formattedCommentBody = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = comment
                                }
                            }
                        }
                    }
                };

                updateSection = new { comment = new[] { new { add = new { body = formattedCommentBody } } } };
            }

            var requestBody = new
            {
                transition = new { id = transitionId },
                update = updateSection,
                fields = fields
            };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/transitions")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The issue transition failed!");
        }

        /// <summary>
        /// Retrieves comments for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve comments.</param>
        /// <param name="startAt">The index of the first comment to return. Defaults to 0.</param>
        /// <param name="maxResults">The maximum number of comments to return. Defaults to 50.</param>
        /// <param name="orderBy">The order in which to return comments. Defaults to null.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The issue comments.</returns>
        public CommentList IssueCommentsGet(string issueKey, int startAt = 0, int maxResults = 50, string orderBy = null, string expand = null)
        {
            var queryParams = new ParameterCollection
            {
                { "startAt", startAt.ToString() },
                { "maxResults", maxResults.ToString() }
            };

            if (orderBy.HasValue())
                queryParams.Add("orderBy", orderBy);
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/comment")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<CommentList>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Adds a comment to an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to which to add the comment.</param>
        /// <param name="body">The body text of the comment.</param>
        /// <param name="visibility">Optional visibility settings for the comment.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The created comment.</returns>
        public Comment IssueCommentAdd(string issueKey, string body, object visibility = null, string expand = null)
        {
            // Format the body as Atlassian Document Format (ADF) for Jira Cloud/Server compatibility
            var formattedBody = new
            {
                type = "doc",
                version = 1,
                content = new[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = body
                            }
                        }
                    }
                }
            };

            var requestBody = new
            {
                body = formattedBody,
                visibility = visibility
            };

            var queryParams = new ParameterCollection();
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/comment")
                .SetQueryParams(queryParams)
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.Created)
                throw new Exception("The comment was not created!");

            return ToType<Comment>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves a specific comment from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the comment.</param>
        /// <param name="commentId">The ID of the comment to retrieve.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The comment.</returns>
        public Comment IssueCommentGet(string issueKey, string commentId, string expand = null)
        {
            var queryParams = new ParameterCollection();
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/comment/{commentId}")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Comment>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Updates a comment on an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the comment.</param>
        /// <param name="commentId">The ID of the comment to update.</param>
        /// <param name="body">The new body text of the comment.</param>
        /// <param name="visibility">Optional visibility settings for the comment.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The updated comment.</returns>
        public Comment IssueCommentUpdate(string issueKey, string commentId, string body, object visibility = null, string expand = null)
        {
            // Format the body as Atlassian Document Format (ADF) for Jira Cloud/Server compatibility
            var formattedBody = new
            {
                type = "doc",
                version = 1,
                content = new[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = body
                            }
                        }
                    }
                }
            };

            var requestBody = new
            {
                body = formattedBody,
                visibility = visibility
            };

            var queryParams = new ParameterCollection();
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/comment/{commentId}")
                .SetQueryParams(queryParams)
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PutWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Comment>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Deletes a comment from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the comment.</param>
        /// <param name="commentId">The ID of the comment to delete.</param>
        public void IssueCommentDelete(string issueKey, string commentId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/comment/{commentId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The comment was not deleted!");
        }

        /// <summary>
        /// Retrieves watchers for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve watchers.</param>
        /// <returns>The issue watchers.</returns>
        public WatchersList IssueWatchersGet(string issueKey)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/watchers")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<WatchersList>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Adds a watcher to an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to which to add the watcher.</param>
        /// <param name="accountId">The account ID of the user to add as a watcher.</param>
        public void IssueWatcherAdd(string issueKey, string accountId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/watchers")
                .SetJsonBody($"\"{accountId}\"")
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The watcher was not added!");
        }

        /// <summary>
        /// Removes a watcher from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue from which to remove the watcher.</param>
        /// <param name="accountId">The account ID of the user to remove as a watcher.</param>
        public void IssueWatcherRemove(string issueKey, string accountId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/watchers")
                .SetQueryParams(new ParameterCollection { { "accountId", accountId } })
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The watcher was not removed!");
        }

        /// <summary>
        /// Retrieves votes for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve votes.</param>
        /// <returns>The issue votes.</returns>
        public Votes IssueVotesGet(string issueKey)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/votes")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Votes>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Adds a vote to an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to vote for.</param>
        public void IssueVoteAdd(string issueKey)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/votes")
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The vote was not added!");
        }

        /// <summary>
        /// Removes a vote from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue from which to remove the vote.</param>
        public void IssueVoteRemove(string issueKey)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/votes")
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The vote was not removed!");
        }

        /// <summary>
        /// Retrieves worklogs for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve worklogs.</param>
        /// <param name="startAt">The index of the first worklog to return. Defaults to 0.</param>
        /// <param name="maxResults">The maximum number of worklogs to return. Defaults to 50.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The issue worklogs.</returns>
        public WorklogList IssueWorklogsGet(string issueKey, int startAt = 0, int maxResults = 50, string expand = null)
        {
            var queryParams = new ParameterCollection
            {
                { "startAt", startAt.ToString() },
                { "maxResults", maxResults.ToString() }
            };

            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/worklog")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<WorklogList>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Adds a worklog to an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to which to add the worklog.</param>
        /// <param name="timeSpentSeconds">The time spent in seconds.</param>
        /// <param name="comment">Optional comment for the worklog.</param>
        /// <param name="started">Optional start date/time for the worklog (ISO 8601 format).</param>
        /// <param name="adjustEstimate">How to adjust the remaining estimate. Options: "new", "leave", "manual", "auto".</param>
        /// <param name="newEstimate">New estimate value when adjustEstimate is "new".</param>
        /// <param name="reduceBy">Amount to reduce estimate by when adjustEstimate is "manual".</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The created worklog.</returns>
        public Worklog IssueWorklogAdd(string issueKey, int timeSpentSeconds, string comment = null,
            string started = null, string adjustEstimate = "auto", string newEstimate = null,
            string reduceBy = null, string expand = null)
        {
            var requestBody = new
            {
                timeSpentSeconds = timeSpentSeconds,
                comment = comment,
                started = started ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            var queryParams = new ParameterCollection();
            if (adjustEstimate.HasValue())
                queryParams.Add("adjustEstimate", adjustEstimate);
            if (newEstimate.HasValue())
                queryParams.Add("newEstimate", newEstimate);
            if (reduceBy.HasValue())
                queryParams.Add("reduceBy", reduceBy);
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/worklog")
                .SetQueryParams(queryParams)
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.Created)
                throw new Exception("The worklog was not created!");

            return ToType<Worklog>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves a specific worklog from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the worklog.</param>
        /// <param name="worklogId">The ID of the worklog to retrieve.</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The worklog.</returns>
        public Worklog IssueWorklogGet(string issueKey, string worklogId, string expand = null)
        {
            var queryParams = new ParameterCollection();
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/worklog/{worklogId}")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Worklog>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Updates a worklog on an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the worklog.</param>
        /// <param name="worklogId">The ID of the worklog to update.</param>
        /// <param name="timeSpentSeconds">The time spent in seconds.</param>
        /// <param name="comment">Optional comment for the worklog.</param>
        /// <param name="started">Optional start date/time for the worklog (ISO 8601 format).</param>
        /// <param name="adjustEstimate">How to adjust the remaining estimate. Options: "new", "leave", "manual", "auto".</param>
        /// <param name="newEstimate">New estimate value when adjustEstimate is "new".</param>
        /// <param name="reduceBy">Amount to reduce estimate by when adjustEstimate is "manual".</param>
        /// <param name="expand">Optional comma-separated list of parameters to expand.</param>
        /// <returns>The updated worklog.</returns>
        public Worklog IssueWorklogUpdate(string issueKey, string worklogId, int timeSpentSeconds,
            string comment = null, string started = null, string adjustEstimate = "auto",
            string newEstimate = null, string reduceBy = null, string expand = null)
        {
            var requestBody = new
            {
                timeSpentSeconds = timeSpentSeconds,
                comment = comment,
                started = started
            };

            var queryParams = new ParameterCollection();
            if (adjustEstimate.HasValue())
                queryParams.Add("adjustEstimate", adjustEstimate);
            if (newEstimate.HasValue())
                queryParams.Add("newEstimate", newEstimate);
            if (reduceBy.HasValue())
                queryParams.Add("reduceBy", reduceBy);
            if (expand.HasValue())
                queryParams.Add("expand", expand);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/worklog/{worklogId}")
                .SetQueryParams(queryParams)
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PutWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Worklog>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Deletes a worklog from an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue containing the worklog.</param>
        /// <param name="worklogId">The ID of the worklog to delete.</param>
        /// <param name="adjustEstimate">How to adjust the remaining estimate. Options: "new", "leave", "manual", "auto".</param>
        /// <param name="newEstimate">New estimate value when adjustEstimate is "new".</param>
        /// <param name="increaseBy">Amount to increase estimate by when adjustEstimate is "manual".</param>
        public void IssueWorklogDelete(string issueKey, string worklogId, string adjustEstimate = "auto",
            string newEstimate = null, string increaseBy = null)
        {
            var queryParams = new ParameterCollection();
            if (adjustEstimate.HasValue())
                queryParams.Add("adjustEstimate", adjustEstimate);
            if (newEstimate.HasValue())
                queryParams.Add("newEstimate", newEstimate);
            if (increaseBy.HasValue())
                queryParams.Add("increaseBy", increaseBy);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/worklog/{worklogId}")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The worklog was not deleted!");
        }

        /// <summary>
        /// Retrieves attachments for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to retrieve attachments.</param>
        /// <returns>The issue attachments.</returns>
        public AttachmentsList IssueAttachmentsGet(string issueKey)
        {
            // Get the issue with attachment field to retrieve attachments
            var issue = IssueGetById(issueKey, "attachment");

            // Convert the issue attachments to AttachmentsList format
            var attachmentsList = new AttachmentsList
            {
                Total = issue.Fields.Attachment?.Count ?? 0,
                MaxResults = issue.Fields.Attachment?.Count ?? 0,
                StartAt = 0,
                IsLast = true,
                Values = issue.Fields.Attachment ?? new List<Attachment>()
            };

            return attachmentsList;
        }

        /// <summary>
        /// Adds an attachment to an issue in Jira.
        /// Note: This method requires file upload capability in the TestApiHttp library.
        /// </summary>
        /// <param name="issueKey">The key of the issue to which to add the attachment.</param>
        /// <param name="filePath">The path to the file to attach.</param>
        /// <param name="fileName">Optional custom filename for the attachment.</param>
        /// <returns>The created attachment information.</returns>
        public AttachmentsList IssueAttachmentAdd(string issueKey, string filePath, string fileName = null)
        {
            if (!System.IO.File.Exists(filePath))
                throw new Exception($"File not found: {filePath}");

            var actualFileName = fileName ?? System.IO.Path.GetFileName(filePath);

            // Note: This assumes the TestApiHttp library supports file uploads
            // The exact implementation may vary based on the library's capabilities
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/attachments")
                .AddHeader("X-Atlassian-Token", "no-check")
                .SetTimeout(RequestTimeoutInSeconds);

            // File upload implementation would go here
            // This is a placeholder as the exact method depends on TestApiHttp capabilities
            throw new NotImplementedException("File upload functionality needs to be implemented based on TestApiHttp library capabilities");
        }

        /// <summary>
        /// Deletes an attachment from Jira.
        /// </summary>
        /// <param name="attachmentId">The ID of the attachment to delete.</param>
        public void IssueAttachmentDelete(string attachmentId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/attachment/{attachmentId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The attachment was not deleted!");
        }

        /// <summary>
        /// Sends a notification for an issue in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue for which to send the notification.</param>
        /// <param name="subject">The subject of the notification.</param>
        /// <param name="textBody">The text body of the notification.</param>
        /// <param name="htmlBody">The HTML body of the notification.</param>
        /// <param name="to">Recipients to send the notification to.</param>
        /// <param name="restrict">Restrictions on who can receive the notification.</param>
        /// <returns>The notification result.</returns>
        public object IssueNotificationSend(string issueKey, string subject, string textBody = null,
            string htmlBody = null, object to = null, object restrict = null)
        {
            var requestBody = new
            {
                subject = subject,
                textBody = textBody,
                htmlBody = htmlBody,
                to = to,
                restrict = restrict
            };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}/notify")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<object>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Creates multiple issues in bulk in Jira.
        /// </summary>
        /// <param name="issueUpdates">Array of issue creation requests.</param>
        /// <returns>The bulk creation results.</returns>
        public BulkOperationResult IssuesBulkCreate(object[] issueUpdates)
        {
            var requestBody = new
            {
                issueUpdates = issueUpdates
            };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/bulk")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.Created)
                throw new Exception("The bulk issue creation failed!");

            return ToType<BulkOperationResult>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Updates multiple issues in bulk in Jira.
        /// </summary>
        /// <param name="issueUpdates">Array of issue update requests.</param>
        /// <returns>The bulk update results.</returns>
        public BulkOperationResult IssuesBulkEdit(object[] issueUpdates)
        {
            var requestBody = new
            {
                issueUpdates = issueUpdates
            };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/bulk")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .PutWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<BulkOperationResult>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Deletes multiple issues in bulk in Jira.
        /// </summary>
        /// <param name="issueIdsOrKeys">Array of issue IDs or keys to delete.</param>
        /// <returns>The bulk deletion results.</returns>
        public BulkOperationResult IssuesBulkDelete(string[] issueIdsOrKeys)
        {
            var requestBody = new
            {
                issueIdsOrKeys = issueIdsOrKeys
            };

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/bulk")
                .SetJsonBody(requestBody)
                .SetTimeout(RequestTimeoutInSeconds)
                .Delete();

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<BulkOperationResult>(jiraResponse.ResponseBody.ContentString);
        }

        #endregion

        #region Projects

        /// <summary>
        /// Cache for storing project metadata to avoid redundant API calls.
        /// Key: Project key, Value: Project metadata response.
        /// </summary>
        private ConcurrentDictionary<string, TestApiResponse> _ProjectMedaData = new ConcurrentDictionary<string, TestApiResponse>();

        /// <summary>
        /// Retrieves and caches project metadata for a given project key.
        /// Subsequent calls for the same project will return cached data.
        /// </summary>
        /// <param name="projectKey">The key of the project.</param>
        /// <returns>The cached or freshly retrieved project metadata.</returns>
        private TestApiResponse ProjectMetaInfoCache(string projectKey)
        {
            if (!_ProjectMedaData.ContainsKey(projectKey))
            {
                _ProjectMedaData.AddOrUpdate(projectKey, IssueCreateMetaDataGet(projectKey));
            }
            return _ProjectMedaData[projectKey];
        }

        /// <summary>
        /// Internal method for paginated project search using the new Jira API v3 approach.
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
        /// </summary>
        private Pagination2<Project> ProjectSearchPaginated(ProjectSearchRequest projectSearchRequest)
        {
            var queryParams = projectSearchRequest
               .GetPropertyValuesV2()
               .TransformKeysToJsonPropertyNames(projectSearchRequest);

            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/search")
                .SetQueryParams(queryParams)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Pagination2<Project>>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves a list of all projects in Jira using the paginated project search API.
        /// This method uses the new Jira API v3 paginated approach internally.
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
        /// </summary>
        /// <returns>A list of all projects.</returns>
        public List<Project> ProjectsGet()
        {
            return SearchFullVersion2<Project, ProjectSearchRequest>(
                new ProjectSearchRequest { },
                (s) => ProjectSearchPaginated(s),
                predicate: null,
                breakSearchOnFirstConditionValid: false).ToList();
        }

        /// <summary>
        /// Retrieves a list of projects in Jira that match the given name or key.
        /// Uses the new paginated search API with query filtering for better performance.
        /// </summary>
        /// <param name="nameOrKey">The name or key of the projects to retrieve.</param>
        /// <returns>A list of projects that match the given name or key.</returns>
        public List<Project> ProjectsGetByNameOrKey(string nameOrKey)
        {
            return SearchFullVersion2<Project, ProjectSearchRequest>(
                new ProjectSearchRequest { Query = nameOrKey },
                (s) => ProjectSearchPaginated(s),
                predicate: p => (p.Key.HasValue() && p.Key.EqualsIgnoreCase(nameOrKey)) ||
                               (p.Name.HasValue() && p.Name.EqualsIgnoreCase(nameOrKey)),
                breakSearchOnFirstConditionValid: false).ToList();
        }

        /// <summary>
        /// Retrieves a project in Jira by its ID.
        /// </summary>
        /// <param name="issueKey">The ID of the project to retrieve.</param>
        /// <returns>The project that matches the given ID.</returns>
        public Project ProjectGetById(string issueKey)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/{issueKey}")
               .SetTimeout(RequestTimeoutInSeconds)
               .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Project>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Searches for projects in Jira based on a query and an optional predicate.
        /// Uses the new paginated project search API with nextPageToken approach.
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
        /// </summary>
        /// <param name="query">The query string to search for projects (filters by name, key, or description).</param>
        /// <param name="predicate">An optional predicate to filter the projects.</param>
        /// <param name="breakSearchOnFirstConditionValid">Indicates whether to stop searching when the first valid condition is met. Defaults to true.</param>
        /// <returns>A list of projects that match the query and predicate.</returns>
        public List<Project> ProjectSearch(string query, Func<Project, bool> predicate = null, bool breakSearchOnFirstConditionValid = true)
        {
            if (JiraApiVersion.ToInteger() <= 2)
                throw new Exception("This is supported on Jira version 3 or above");

            return SearchFullVersion2<Project, ProjectSearchRequest>(
                new ProjectSearchRequest { Query = query },
                (s) => ProjectSearchPaginated(s),
                predicate,
                breakSearchOnFirstConditionValid).ToList();
        }

        #endregion

        #region Users

        /// <summary>
        /// Retrieves the account information of the currently logged-in user in Jira.
        /// </summary>
        /// <returns>An Assignee object containing the account information of the logged-in user.</returns>
        public Assignee LoggedInUserAccount()
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/user/search")
              .SetQueryParams(new ParameterCollection { { "query", _username } })
              .SetTimeout(RequestTimeoutInSeconds)
              .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ((List<Assignee>)ToType<List<Assignee>>(jiraResponse.ResponseBody.ContentJson))?.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the account information of a user in Jira by their account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the user to retrieve.</param>
        /// <returns>An Assignee object containing the account information of the specified user.</returns>
        public Assignee UserAccountGet(string accountId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/user")
                .SetQueryParams(new ParameterCollection { { "accountId", accountId } })
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ToType<Assignee>(jiraResponse.ResponseBody.ContentJson);
        }

        #endregion

        /// <summary>
        /// Retrieves the versions of a project in Jira.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<Version> VersionsGet(string projectId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/{projectId}/versions")
              .SetTimeout(RequestTimeoutInSeconds)
              .GetWithRetry(assertOk: AssertResponseStatusOk,
                  timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                  retryOption: RequestRetryTimes,
                  httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                  retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ToType<List<Version>>(jiraResponse.ResponseBody.ContentJson);
        }

        /// <summary>
        /// Retrieves the version information in Jira by the version ID.
        /// </summary>
        /// <param name="versionId">The ID of the version to retrieve.</param>
        /// <returns>A Version object containing the information of the specified version.</returns>
        public Version VersionGet(string versionId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/version/{versionId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ToType<Version>(jiraResponse.ResponseBody.ContentJson);
        }

        /// <summary>
        /// Retrieves the components available in the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<ProjectComponent> ComponentsGet(string projectId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/{projectId}/components")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ToType<List<ProjectComponent>>(jiraResponse.ResponseBody.ContentJson);
        }

        /// <summary>
        /// Retrieves the component information in Jira by the component ID.
        /// </summary>
        /// <param name="componentId">The ID of the component to retrieve.</param>
        /// <returns>A ProjectComponent object containing the information of the specified component.</returns>
        public ProjectComponent ComponentGet(string componentId)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/component/{componentId}")
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                    timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                    retryOption: RequestRetryTimes,
                    httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                    retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();

            return ToType<ProjectComponent>(jiraResponse.ResponseBody.ContentJson);
        }

        /// <summary>
        /// Generic internal method for performing paginated searches using SearchRequestBase2 with startAt-based pagination.
        /// Note: This method is designed for endpoints that use numeric startAt offsets (old pagination approach).
        /// For nextPageToken-based pagination (new Jira API), use IssueSearchWithPagination instead.
        /// </summary>
        /// <typeparam name="T">The type of items to return in the search results.</typeparam>
        /// <typeparam name="M">The search request type that inherits from SearchRequestBase2.</typeparam>
        /// <param name="searchQuery">The search query parameters including startAt and maxResults.</param>
        /// <param name="search">The function to execute the search and return paginated results.</param>
        /// <param name="predicate">Optional predicate to filter the results.</param>
        /// <param name="breakSearchOnFirstConditionValid">If true, stops searching when the first matching result is found.</param>
        /// <returns>A list of items that match the search criteria.</returns>
        internal IList<T> SearchFullVersion2<T, M>(
            M? searchQuery,
            Func<M, Pagination2<T>> search,
            Func<T, bool>? predicate = null,
            bool breakSearchOnFirstConditionValid = true) where M : SearchRequestBase2
        {
            var results = new List<T>();
            if (searchQuery == null) return results.ToList();

            // Use provided MaxResults or default to 100
            if (!searchQuery.MaxResults.HasValue || searchQuery.MaxResults.Value <= 0)
            {
                searchQuery.MaxResults = 50;
            }

            searchQuery.StartAt = 0;

            do
            {
                var resp = search(searchQuery);
                if (predicate != null)
                {
                    foreach (var value in resp.PaginatedItems)
                    {
                        if (predicate(value) == true)
                        {
                            results.Add(value);
                            if (breakSearchOnFirstConditionValid)
                            {
                                return results.ToList();
                            }
                        }
                    }
                }
                else
                {
                    resp.PaginatedItems.Iter(r => results.Add(r));
                }

                // Check if there are more pages using isLast or nextPageToken
                if (resp.IsLast || string.IsNullOrEmpty(resp.NextPageToken))
                {
                    break;
                }

                // Update startAt for the next page
                searchQuery.StartAt = long.Parse(resp.NextPageToken);
            }
            while (true);

            return results.ToList();
        }

        /// <summary>
        /// Generic internal method for performing paginated searches using dictionary-based query parameters.
        /// This method handles pagination with startAt and maxResults parameters for older Jira API endpoints.
        /// </summary>
        /// <typeparam name="T">The type of items to return in the search results.</typeparam>
        /// <param name="searchQuery">Dictionary containing query parameters including startAt and maxResults.</param>
        /// <param name="search">The function to execute the search and return paginated results.</param>
        /// <param name="predicate">Optional predicate to filter the results.</param>
        /// <param name="breakSearchOnFirstConditionValid">If true, stops searching when the first matching result is found.</param>
        /// <returns>A list of items that match the search criteria.</returns>
        internal IList<T> SearchFull<T>(
            IDictionary<string, string> searchQuery,
            Func<IDictionary<string, string>, Pagination<T>> search,
            Func<T, bool> predicate = null,
            bool breakSearchOnFirstConditionValid = true)
        {
            var results = new ConcurrentBag<T>();
            int maxResults = 50;
            if (searchQuery == null) searchQuery = new Dictionary<string, string>();
            if (searchQuery.ContainsKey("maxResults")) searchQuery["maxResults"] = maxResults.ToString(); else searchQuery.Add("maxResults", maxResults.ToString());
            if (searchQuery.ContainsKey("startAt")) searchQuery["startAt"] = "0"; else searchQuery.Add("startAt", "0");

            var resp = search(searchQuery);

            if (predicate != null)
            {
                foreach (var value in resp.PaginatedItems)
                {
                    if (predicate(value) == true)
                    {
                        results.Add(value);
                        if (breakSearchOnFirstConditionValid)
                        {
                            return results.ToList();
                        }
                    }
                }
            }
            else
            {
                resp.PaginatedItems.Iter(r => results.Add(r));
            }

            if (resp.total > resp.PaginatedItems.Count)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var totalPages = new PagingModel { TotalItems = (int)resp.total, PageSize = (int)resp.maxResults }.TotalPagesAvailable;
                int count = 0;

                try
                {
                    Parallel.For(1, totalPages, new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken }, i =>
                    {
                        lock (_lock) { count++; }
                        var currentSearchQry = searchQuery.DeepClone();
                        currentSearchQry["startAt"] = (i * maxResults).ToString();

                        PjUtility.Log($"Trying to read {count} of {totalPages} starting at {currentSearchQry["startAt"]}");
                        var searchResult = search(currentSearchQry);

                        if (predicate != null)
                        {
                            foreach (var value in searchResult.PaginatedItems)
                            {
                                if (predicate(value) == true)
                                {
                                    results.Add(value);
                                    if (breakSearchOnFirstConditionValid)
                                    {
                                        cancellationTokenSource.Cancel();
                                    }
                                }
                            }
                        }
                        else
                        {
                            (searchResult.values ?? searchResult.issues).Iter(r => results.Add(r));
                        }
                    });
                }
                catch (OperationCanceledException e)
                {
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }
            return results.ToList();
        }

        /// <summary>
        /// Helper method to check if a JToken is empty (null, empty string, empty array, or empty object).
        /// Used when extracting dynamic fields from issue responses.
        /// </summary>
        /// <param name="token">The JToken to check.</param>
        /// <returns>True if the token is empty, false otherwise.</returns>
        private bool IsEmptyToken(JToken token)
        {
            return token.Type == JTokenType.Null ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues);
        }
    }
}
