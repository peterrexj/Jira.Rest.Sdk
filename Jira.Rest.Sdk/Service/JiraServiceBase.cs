using Jira.Rest.Sdk.Dtos;
using Newtonsoft.Json;
using Pj.Library;
using System;
using System.Collections.Generic;
using System.Net;
using TestAny.Essentials.Api;
using TestAny.Essentials.Core;

namespace Jira.Rest.Sdk
{
    public abstract class JiraServiceBase
    {
        protected readonly object _lock = new object();
        protected string _logPrefix;
        protected string _appName = "Jira";
        protected string _appFullEndpoint;
        protected string _username;
        protected string _password;
        private readonly bool _isCloudVersion = true;
        private static bool _loginVerified = false;
        protected string _proxyKeyName;
        protected string _authToken;

        public string JiraUrl { get; set; }
        public string JiraApiVersion { get; set; }
        public string FolderSeparator { get; set; }

        protected int PageSizeSearch;
        protected int PageSizeSearchResult;
        protected int RequestRetryTimes;
        protected int TimeToSleepBetweenRetryInMilliseconds;
        protected bool AssertResponseStatusOk;
        protected HttpStatusCode[] ListOfResponseCodeOnFailureToRetry;
        protected bool RetryOnRequestTimeout;
        protected int RequestTimeoutInSeconds;

        protected JiraServiceBase(string appUrl,
            string serviceUsername,
            string servicePassword,
            bool isCloudVersion,
            string jiraApiVersion,
            string folderSeparator,
            string logPrefix,
            int pageSizeSearchResult,
            int requestRetryTimes,
            int timeToSleepBetweenRetryInMilliseconds,
            bool assertResponseStatusOk,
            HttpStatusCode[] listOfResponseCodeOnFailureToRetry,
            int requestTimeoutInSeconds,
            bool retryOnRequestTimeout,
            string proxyKeyName,
            string authToken)
        {
            SetBaseValues(appUrl, serviceUsername, servicePassword, isCloudVersion, 
                jiraApiVersion, folderSeparator, logPrefix, pageSizeSearchResult,
                requestRetryTimes, timeToSleepBetweenRetryInMilliseconds, assertResponseStatusOk, 
                listOfResponseCodeOnFailureToRetry, requestTimeoutInSeconds, retryOnRequestTimeout,
                proxyKeyName, authToken);
        }

        private void SetBaseValues(string appUrl,
            string serviceUsername,
            string servicePassword,
            bool isCloudVersion,
            string jiraApiVersion,
            string folderSeparator,
            string logPrefix,
            int pageSizeSearchResult,
            int requestRetryTimes,
            int timeToSleepBetweenRetryInMilliseconds,
            bool assertResponseStatusOk,
            HttpStatusCode[] listOfResponseCodeOnFailureToRetry,
            int requestTimeoutInSeconds,
            bool retryOnRequestTimeout,
            string proxyKeyName,
            string authToken)
        {
            if (appUrl.IsEmpty())
            {
                throw new Exception($"The url to the {_appName} is required");
            }
            if (appUrl.ContainsDomain() == false)
            {
                throw new Exception($"The url to the {_appName} is not in the correct format");
            }

            JiraUrl = appUrl.GetDomain();
            JiraApiVersion = jiraApiVersion.ReplaceMultiple("", "/", @"\");
            _username = serviceUsername;
            _password = servicePassword;
            FolderSeparator = folderSeparator;

            _appFullEndpoint = JiraUrl;
            _logPrefix = logPrefix;
            _proxyKeyName = proxyKeyName;
            _authToken = authToken;
            PageSizeSearch = pageSizeSearchResult;
            RequestRetryTimes = requestRetryTimes;
            TimeToSleepBetweenRetryInMilliseconds = timeToSleepBetweenRetryInMilliseconds;
            AssertResponseStatusOk = assertResponseStatusOk;
            ListOfResponseCodeOnFailureToRetry = listOfResponseCodeOnFailureToRetry;
            RetryOnRequestTimeout = retryOnRequestTimeout;
            RequestTimeoutInSeconds = requestTimeoutInSeconds;
            TestAnyAppConfig.DefaultApiResponseTimeoutWaitPeriodInSeconds = requestTimeoutInSeconds;
        }

        protected void Log(string message) => PjUtility.Log($"{_logPrefix}{message}");
        protected static T ToType<T>(dynamic content) => SerializationHelper.ToType<T>(content);
        protected static T ToType<T>(string content) => SerializationHelper.ToType<T>(content);
        protected static string ToJson(object content) => JsonConvert.SerializeObject(content);

        public virtual bool CanConnect
        {
            get
            {
                if (_loginVerified == false)
                {
                    _loginVerified = CanLogin();
                }
                return _loginVerified;
            }
        }

        protected virtual bool CanLogin()
        {
            TestApiResponse testResponse = null;
            var statusHealthEndpoint = _isCloudVersion ? "/status" : $"/rest/api/{JiraApiVersion}/serverInfo";
            for (int i = 0; i < 10; i++)
            {
                Log($"Jira health status to {_appFullEndpoint}{statusHealthEndpoint} check");
                try
                {
                    testResponse = new TestApiHttp()
                         .SetEnvironment(_appFullEndpoint)
                         .PrepareRequest(statusHealthEndpoint)
                         .AddBasicAuthorizationHeader(_username, _password)
                         .AddBearerAuthorizationHeader(_authToken)
                         .SetNtmlAuthentication()
                         .ProxyRequired(_proxyKeyName.HasValue())
                         .AddProxy(_proxyKeyName)
                         .Get();
                }
                catch (Exception)
                {
                }
               
                if (testResponse?.ResponseCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Log($"Failed to communicate with {_appName} as the user pin may be incorrect");
                    throw new Exception($"Failed to communicate with {_appName} as the user pin may be incorrect");
                }
                else if (testResponse?.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Log($"{_appName} health status to {_appFullEndpoint}{statusHealthEndpoint} check failed with response code: {testResponse?.ResponseCode.ToString()}");
                    System.Threading.Thread.Sleep(2000);
                }
            }

            Log($"Could not communicate with the {_appName} server. Response code: {testResponse?.ResponseCode.ToString()}, Response Body: {testResponse?.ResponseBody?.ContentString}");
            throw new Exception($"Could not communicate with the {_appName} server. Response code: {testResponse?.ResponseCode.ToString()}, Response Body: {testResponse?.ResponseBody?.ContentString}");
        }
        protected virtual void CheckConnection()
        {
            if (!CanConnect)
            {
                throw new Exception($"Cannot communicate to the {_appName}");
            }
        }

        protected TestApiRequest OpenRequest(string requestUrl)
        {
            CheckConnection();

            return new TestApiHttp()
                .SetEnvironment(_appFullEndpoint)
                .PrepareRequest(requestUrl)
                .AddBasicAuthorizationHeader(_username, _password)
                .AddBearerAuthorizationHeader(_authToken)
                .SetNtmlAuthentication()
                .ProxyRequired(_proxyKeyName.HasValue())
                .AddProxy(_proxyKeyName);
        }


        protected long SearchCount<T>(IDictionary<string, string> searchQuery,
            Func<IDictionary<string, string>, Pagination<T>> search)
        {
            if (searchQuery.ContainsKey("maxResults")) searchQuery["maxResults"] = "1"; else searchQuery.Add("maxResults", "1");
            if (searchQuery.ContainsKey("startAt")) searchQuery["startAt"] = "0"; else searchQuery.Add("startAt", "0");

            var resp = search(searchQuery);

            if (resp != null && resp.total > 0)
            {
                return resp.total;
            }

            return 0;
        }
    }
}
