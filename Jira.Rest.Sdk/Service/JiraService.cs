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

        private IList<Project> ProjectsGet(IDictionary<string, string> projectSearchRequest)
        {
            if (projectSearchRequest?.ContainsKey("fields") == false)
            {
                projectSearchRequest.Add("fields", "key,name,folder,status,priority,component,owner,estimatedTime,labels,customFields,issueLinks");
            }
            if (projectSearchRequest?.ContainsKey("maxResults") == false)
            {
                projectSearchRequest.Add("maxResults", PageSizeSearch.ToString());
            }
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/search")
                .SetQueryParams(projectSearchRequest)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<IList<Project>>(jiraResponse.ResponseBody.ContentString);
        }

        #region Issues

        /// <summary>
        /// Creates a new issue in Jira.
        /// </summary>
        /// <param name="projectKey">The key of the project in which to create the issue.</param>
        /// <param name="issueType">The type of the issue to create (e.g., Bug, Task).</param>
        /// <param name="summary">A brief summary of the issue.</param>
        /// <param name="description">A detailed description of the issue.</param>
        /// <param name="priority">The priority of the issue (e.g., High, Medium, Low).</param>
        /// <param name="parentKey">The key of the parent issue, if any. Defaults to null.</param>
        /// <returns>The created issue.</returns>
        public Issue IssueCreate(string projectKey, string issueType,
            string summary,
            string description,
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
                    Description = description,
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
        /// Creates a link between two issues in Jira.
        /// </summary>
        /// <param name="linkType">The type of link to create (e.g., "blocks", "relates to").</param>
        /// <param name="outwardIssueKey">The key of the outward issue.</param>
        /// <param name="inwardIssueKey">The key of the inward issue.</param>
        /// <param name="issueInfo">Optional additional information about the issue link. Defaults to null.</param>
        public void IssueLink(string linkType, string outwardIssueKey, string inwardIssueKey, Issue issueInfo = null)
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
                        Type = new Dtos.Type { Name = linkType },
                        InwardIssue = new InwardIssue { Key = inwardIssueKey },
                        OutwardIssue = new OutwardIssue { Key = outwardIssueKey },
                        Comment = new Comment
                        {
                            Body = $"Automation: Link created between {inwardIssueKey} and {outwardIssueKey} of type {linkType}",
                        }
                    };

                    var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issueLink")
                        .SetJsonBody(requestModel)
                        .SetTimeout(RequestTimeoutInSeconds)
                        .PostWithRetry(assertOk: AssertResponseStatusOk,
                           timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                           retryOption: RequestRetryTimes,
                           httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                           retryOnRequestTimeout: RetryOnRequestTimeout);
                }
            }
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
        /// Assigns an issue to a user by their username in Jira.
        /// </summary>
        /// <param name="issueKey">The key of the issue to be assigned.</param>
        /// <param name="username">The username of the user to whom the issue will be assigned.</param>
        public void IssueAssigneeByName(string issueKey, string username)
        {
            var reqBody = new { name = username };

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

            var reqBody = new UpdateIssueRequest { Update = new Update { } };
            reqBody.Update.Description = new List<dynamic>();
            reqBody.Update.Description.Add(new { set = description });


            var jiraResponse = OpenRequest($"/rest/api/2/issue/{issueKey}")
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

        internal Pagination<Issue> IssueSearch(IDictionary<string, string> issueSearchRequest)
        {
            //Using POST to handle large query string
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/search/jql")
                .SetJsonBody(issueSearchRequest)
                //.SetQueryParams(issueSearchRequest)
                .SetTimeout(RequestTimeoutInSeconds)
                .PostWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Pagination<Issue>>(jiraResponse.ResponseBody.ContentJson);
        }

        //"TODO: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
        //"TODO: https://developer.atlassian.com/changelog/#CHANGE-2046

        /// <summary>
        /// Searches for issues in Jira using JQL (Jira Query Language).
        /// Reference: https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-post
        /// </summary>
        /// <param name="jql">The JQL query string to use for searching issues.</param>
        /// <param name="fields">A list of fields to return for each issue. By default, all navigable fields are returned.</param>
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
            return SearchFullVersion2<Issue>(
                new IssueSearchRequest { Jql = jql, Fields = fields, FieldsByKeys = fieldsByKeys, Expand = expand, Properties = properties, ReconcileIssues = reconcileIssues }.GetPropertyValuesV2(),
                (s) => IssueSearch(s), predicate, breakSearchOnFirstConditionValid).ToList();
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

        #endregion

        #region Projects

        /// <summary>
        /// Retrieves a list of all projects in Jira.
        /// </summary>
        /// <returns>A list of all projects.</returns>
        public List<Project> ProjectsGet()
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project")
                .WithJsonResponse()
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<List<Project>>(jiraResponse.ResponseBody.ContentString);
        }

        /// <summary>
        /// Retrieves a list of projects in Jira that match the given name or key.
        /// </summary>
        /// <param name="nameOrKey">The name or key of the projects to retrieve.</param>
        /// <returns>A list of projects that match the given name or key.</returns>
        public List<Project> ProjectsGetByNameOrKey(string nameOrKey)
        {
            return ProjectsGet()
                .Where(p => (p.Key.HasValue() && p.Key.EqualsIgnoreCase(nameOrKey)) || (p.Name.HasValue() && p.Name.EqualsIgnoreCase(nameOrKey)))
                .ToList();
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

        private Pagination<Project> ProjectSearch(IDictionary<string, string> issueSearchRequest)
        {
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/project/search")
                .SetQueryParams(issueSearchRequest)
                .SetTimeout(RequestTimeoutInSeconds)
                .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Pagination<Project>>(jiraResponse.ResponseBody.ContentJson);
        }

        /// <summary>
        /// Searches for projects in Jira based on a query and an optional predicate.
        /// </summary>
        /// <param name="query">The query string to search for projects.</param>
        /// <param name="predicate">An optional predicate to filter the projects.</param>
        /// <param name="breakSearchOnFirstConditionValid">Indicates whether to stop searching when the first valid condition is met. Defaults to true.</param>
        /// <returns>A list of projects that match the query and predicate.</returns>
        public List<Project> ProjectSearch(string query, Func<Project, bool> predicate = null, bool breakSearchOnFirstConditionValid = true)
        {
            if (JiraApiVersion.ToInteger() <= 2)
                throw new Exception("This is supported on Jira version 3 or above");

            return SearchFull<Project>(
                new { query = query }.GetPropertyValuesV2(),
                (s) => ProjectSearch(s), predicate, breakSearchOnFirstConditionValid).ToList();
        }

        private ConcurrentDictionary<string, TestApiResponse> _ProjectMedaData = new ConcurrentDictionary<string, TestApiResponse>();
        private TestApiResponse ProjectMetaInfoCache(string projectKey)
        {
            if (!_ProjectMedaData.ContainsKey(projectKey))
            {
                _ProjectMedaData.AddOrUpdate(projectKey, IssueCreateMetaDataGet(projectKey));
            }
            return _ProjectMedaData[projectKey];
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

        internal IList<T> SearchFullVersion2<T>(
            IDictionary<string, string>? searchQuery,
            Func<IDictionary<string, string>, Pagination<T>> search,
            Func<T, bool>? predicate = null,
            bool breakSearchOnFirstConditionValid = true)
        {
            var results = new ConcurrentBag<T>();
            var maxResults = 50;
            searchQuery ??= new Dictionary<string, string>();
            if (searchQuery.ContainsKey("maxResults")) searchQuery["maxResults"] = maxResults.ToString(); else searchQuery.Add("maxResults", maxResults.ToString());
            if (searchQuery.ContainsKey("nextPageToken")) searchQuery.Remove("nextPageToken");

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

        private bool IsEmptyToken(JToken token)
        {
            return token.Type == JTokenType.Null ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues);
        }
    }
}
