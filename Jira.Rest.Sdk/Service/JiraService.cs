using Jira.Rest.Sdk.Dtos;
using Pj.Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            string proxyKeyName = "")
                : base(appUrl, serviceUsername, servicePassword, isCloudVersion, 
                      jiraApiVersion, folderSeparator, logPrefix, pageSizeSearchResult,
                      requestRetryTimes, timeToSleepBetweenRetryInMilliseconds, assertResponseStatusOk, 
                      listOfResponseCodeOnFailureToRetry, requestTimeoutInSeconds, retryOnRequestTimeout,
                      proxyKeyName)
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

        #region Issue Create/Update
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

        public void IssueDelete(string issueIdOrKey, bool deleteSubtasks = false)
        {
            var jiraResponse = OpenRequest($" /rest/api/{JiraApiVersion}/issue/{issueIdOrKey}")
                .SetQueryParams(new ParameterCollection { { "deleteSubtasks", deleteSubtasks } })
                .Delete();

            if (jiraResponse.ResponseCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("The component was not updated!");
        }
        #endregion

        

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
        public List<Project> ProjectsGetByNameOrKey(string nameOrKey)
        {
            return ProjectsGet()
                .Where(p => (p.Key.HasValue() && p.Key.EqualsIgnoreCase(nameOrKey)) || (p.Name.HasValue() && p.Name.EqualsIgnoreCase(nameOrKey)))
                .ToList();
        }

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
        public List<Project> ProjectSearch(string query, Func<Project, bool> predicate = null, bool breakSearchOnFirstConditionValid = true)
        {
            if (JiraApiVersion.ToInteger() <= 2)
                throw new Exception("This is supported on Jira version 3 or above");

            return SearchFull<Project>(
                new { query = query }.GetPropertyValuesV2(),
                (s) => ProjectSearch(s), predicate, breakSearchOnFirstConditionValid).ToList();
        }


        public Issue IssueGetById(string issueKey)
        {
            Log($"Trying to get jira issue [{issueKey}]");
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/issue/{issueKey}")
               .SetTimeout(RequestTimeoutInSeconds)
               .GetWithRetry(assertOk: AssertResponseStatusOk,
                   timeToSleepBetweenRetryInMilliseconds: TimeToSleepBetweenRetryInMilliseconds,
                   retryOption: RequestRetryTimes,
                   httpStatusCodes: ListOfResponseCodeOnFailureToRetry,
                   retryOnRequestTimeout: RetryOnRequestTimeout);

            jiraResponse.AssertResponseStatusForSuccess();
            return ToType<Issue>(jiraResponse.ResponseBody.ContentString);
        }
        internal Pagination<Issue> IssueSearch(IDictionary<string, string> issueSearchRequest)
        {
            //Using POST to handle large query string
            var jiraResponse = OpenRequest($"/rest/api/{JiraApiVersion}/search")
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
        public List<Issue> IssueSearch(string jql, Func<Issue, bool> predicate = null, bool breakSearchOnFirstConditionValid = true)
        {
            return SearchFull<Issue>(
                new IssueSearchRequest { jql = jql }.GetPropertyValuesV2(),
                (s) => IssueSearch(s), predicate, breakSearchOnFirstConditionValid).ToList();
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

        private ConcurrentDictionary<string, TestApiResponse> _ProjectMedaData = new ConcurrentDictionary<string, TestApiResponse>();

        private TestApiResponse ProjectMetaInfoCache(string projectKey)
        {
            if (!_ProjectMedaData.ContainsKey(projectKey))
            {
                _ProjectMedaData.AddOrUpdate(projectKey, IssueCreateMetaDataGet(projectKey));
            }
            return _ProjectMedaData[projectKey];
        }

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
    }
}
