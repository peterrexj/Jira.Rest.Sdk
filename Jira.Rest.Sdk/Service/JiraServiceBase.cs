using Newtonsoft.Json;
using Pj.Library;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
        private readonly bool _isCloudVersion = false;
        private static bool _loginVerified = false;

        public string JiraUrl { get; set; }
        public string JiraApiVersion { get; set; }
        public string FolderSeparator { get; set; }

        protected int PageSizeSearch;
        protected int PageSizeSearchResult;
        protected int RequestRetryTimes;
        protected int TimeToSleepBetweenRetryInMilliseconds;
        protected bool AssertResponseStatusOk;
        protected HttpStatusCode[] ListOfResponseCodeOnFailureToRetry;

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
            int requestTimeoutInSeconds)
        {
            SetBaseValues(appUrl, serviceUsername, servicePassword, isCloudVersion, jiraApiVersion, folderSeparator, logPrefix, pageSizeSearchResult,
                requestRetryTimes, timeToSleepBetweenRetryInMilliseconds, assertResponseStatusOk, listOfResponseCodeOnFailureToRetry, requestTimeoutInSeconds);
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
            int requestTimeoutInSeconds)
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
            PageSizeSearch = pageSizeSearchResult;
            RequestRetryTimes = requestRetryTimes;
            TimeToSleepBetweenRetryInMilliseconds = timeToSleepBetweenRetryInMilliseconds;
            AssertResponseStatusOk = assertResponseStatusOk;
            ListOfResponseCodeOnFailureToRetry = listOfResponseCodeOnFailureToRetry;
            TestAnyAppConfig.DefaultApiResponseTimeoutWaitPeriodInSeconds = requestTimeoutInSeconds;
        }

        protected void Log(string message) => Console.WriteLine($"{_logPrefix}{message}");
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
                         .SetNtmlAuthentication()
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
                .SetNtmlAuthentication();
        }

       
    }
}
