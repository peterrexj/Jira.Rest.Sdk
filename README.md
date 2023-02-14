Jira.Rest.Sdk
========

SDK using Jira REST to query Jira application using Rest endpoints. Manage your Jira process from query, create and update issues. Integrate with you existing automation solution or process that will manage both Jira Server and Cloud based application.

The request and response objects are having proper DTOS (data transfer or model objects) defined within this package.

## How to Use

### Connect to service

```C#
     //Connect to cloud hosted Jira service
     var jiraService = new JiraService("jira url", "username", "password", isCloudVersion: true);

     //Connect to cloud hosted Jira service
     var jiraService = new JiraService("jira url", "username", "password", isCloudVersion: false);

     //Connect to cloud hosted Jira service using bearer token
     var jiraService = new JiraService("jira url", 
          serviceUsername: "", 
          servicePassword: "", 
          isCloudVersion: true,
          authToken: "bearer token");

     //Get a test case by Key
     var issue = jiraService.IssueGetById("POC-100");

     //Get a list of issues matching your jql
     var issue = jiraService.IssueSearch("<your jql>");

     //Get project by custom filters on any properties
     var project = jiraService.ProjectsGet(p => p.Name.EqualsIgnoreCase("poc")).FirstOrDefault();

     //Create a Issue
     var newIssue = jiraService.IssueCreate(projectKey: "POC", 
                    issueType: "Story", 
                    summary: "Build new interface for model B", 
                    description: "Provide your detailed description for the issue", 
                    priority: "High", 
                    parentKey: "POC-99")
```

### Current Features

     - ProjectsGet
     - ProjectsGet  = with options to search by any project's field property
     - IssueCreate
     - IssueLink
     - IssueLabelAdd
     - IssueAssigneeByAccountId
     - IssueAssigneeByName
     - IssueComponentUpdate
     - IssueAffectedVersionUpdate
     - IssueAffectedVersionRemove
     - IssueFixVersionUpdate
     - IssueFixVersionRemove
     - IssueDescriptionUpdate
     - IssueGetById
     - IssueSearch = with option to search by any issue or its child's field property
     
### Custom Control on the Service - Cloud / Server

There are some level of custom customization available on the service that can be passed on via the constuctor.

1. restApiVersion - Rest API version for the Zeyphr Service (default value: 'v2')
2. folderSeparator - Folder separator string (default value: '/')
3. logPrefix - Prefix text that will be added to all the logs generated from this service (default value: 'Jira: ')
4. pageSizeSearchResult - Page size for search request (default value: '50')
5. requestRetryTimes - Number of time to retry when there is a network failure (default value: '1'). You can increase the number of times to retry based on your infrastructure if there are chance for a request to fail randomly
6. timeToSleepBetweenRetryInMilliseconds - Time to sleep in milliseconds between each time a call is retry (default value: '1000'). Applied only when requestRetryTimes is more than 1
7. assertResponseStatusOk - True/False whether the response code status from the server needs to be asserted for OK (default value 'true')
8. listOfResponseCodeOnFailureToRetry - Any of these status code matched from response will then use for retry the request. For example Proxy Authentication randomly failing can be then used to retry (default value 'null' which means it is not checking any response code for fail retry)
9. requestTimeoutInSeconds - You can increase the response wait time from server based on your server performance, load or network traffic or speed (default value: 300)

A scenario where you have network issues and you want to retry operation, then try this
```C#
     //Connect to cloud hosted jira service
     var jService = new JiraService("jira url", "username", "password", isCloudVersion: true,
          requestRetryTimes: 20,
          timeToSleepBetweenRetryInMilliseconds: 1500,
          assertResponseStatusOk: true,
          listOfResponseCodeOnFailureToRetry: new System.Net.HttpStatusCode []  { System.Net.HttpStatusCode.ProxyAuthenticationRequired  });
```
The above will apply an automatic retry of a maximum 20 times, giving itself a break of 1500 milliseconds between each request made to Jira that fails with a response code of HttpStatusCode.ProxyAuthenticationRequired