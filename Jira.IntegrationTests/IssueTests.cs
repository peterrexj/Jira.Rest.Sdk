using NUnit.Framework;
using Pj.Library;
using System.Linq;
using System.Net;

namespace Jira.IntegrationTests;

internal class IssueTests : TestBase
{
    [TestCase]
    public void Should_Create_Issue()
    {
        var issueSummary = $"{EnvironmentVariables.IssueNamePrefix}_{DateTimeEx.GetDateTimeReadable()}";
        var issue = _service.IssueCreate(EnvironmentVariables.ProjectKey, "Story", issueSummary, "High");
        Assert.IsNotNull(issue);
        Assert.IsTrue(issue.Id.HasValue());
        Assert.IsTrue(issue.Id.ToLong() > 0);
        Assert.IsTrue(issue.Key.HasValue());
    }

    [TestCase]
    public void Should_Delete_Issue()
    {
        Assert.IsNotEmpty(EnvironmentVariables.IssueCleanFilter, "The value of issue filter is required");
        var issues = _service.IssueSearch(EnvironmentVariables.IssueCleanFilter);
        foreach (var item in issues)
        {
            if (item.Fields.Summary.StartsWith(EnvironmentVariables.IssueNamePrefix))
            {
                _service.IssueDelete(item.Key);
            }
        }
        var issuesNew = _service.IssueSearch(EnvironmentVariables.IssueCleanFilter);
        Assert.IsNotNull(issuesNew);
        Assert.AreEqual(issuesNew.Count, 0);
    }

    [TestCase]
    public void Should_Get_Issue_By_Id()
    {
        var issueKey = EnvironmentVariables.IssueKey;
        var issue = _service.IssueGetById(issueKey);
        Assert.IsNotNull(issue);
        Assert.AreEqual(issueKey, issue.Key);
    }

    [TestCase]
    public void Should_Filter_Issues()
    {
        Assert.IsNotEmpty(EnvironmentVariables.IssueFilter, "The value of issue filter is required");
        var issues = _service.IssueSearch(EnvironmentVariables.IssueFilter);
        Assert.IsNotNull(issues);
        Assert.Greater(issues.Count, 0, $"There should be at least one issue returned from the server by this filter [{EnvironmentVariables.IssueFilter}]");

        //at least 50% of the issues should have properties with values
        foreach (var issue in issues)
        {
            var dynamicObj = JsonHelper.ConvertComplexJsonDataToDictionary(SerializationHelper.SerializeToJson(issue));
            var properties = dynamicObj.Where(p => p.Value.HasValue()).ToList();
            Assert.GreaterOrEqual(properties.Count, dynamicObj.Count * 0.5);
        }
    }

    //This test requires the user to have admin rights on the jira project
    [TestCase]
    public void Should_Get_Issue_Metadata()
    {
        var issueKey = EnvironmentVariables.IssueKey;
        var metadata = _service.IssueMetadataGet(issueKey);
        Assert.IsNotNull(metadata);
        Assert.Greater(metadata.Count, 0);
    }

    [TestCase]
    public void Should_Get_Issue_Create_MetaData()
    {
        var projectKey = EnvironmentVariables.ProjectKey;
        var issueType = "Story"; // Replace with actual issue type
        var metadata = _service.IssueCreateMetaDataGet(projectKey, issueType);
        Assert.IsNotNull(metadata);
        Assert.AreEqual(metadata.ResponseCode, HttpStatusCode.OK);
        Assert.IsNotNull(metadata.ResponseBody.ContentString);
    }

    [TestCase]
    public void Should_Get_Issue_Link_Types_Metadata()
    {
        var metadata = _service.IssueLinkTypesMetadataGet();
        Assert.IsNotNull(metadata);
        Assert.AreEqual(metadata.ResponseCode, HttpStatusCode.OK);
        Assert.IsNotNull(metadata.ResponseBody.ContentString);
    }

    [TestCase]
    public void Should_Get_Issue_Search_Approximate_Count()
    {
        var jql = EnvironmentVariables.IssueFilter;
        var approximateCount = _service.IssueSearchApproximateCount(jql);
        Assert.Greater(approximateCount, 0, "The approximate count of issues should be greater than zero.");
    }


    [TestCase]
    public void Should_Create_Issue_Link()
    {
        var linkType = "blocks"; // Replace with actual link type
        var outwardIssueKey = ""; // Replace with actual outward issue key
        var inwardIssueKey = ""; // Replace with actual inward issue key
        _service.IssueLink(linkType, outwardIssueKey, inwardIssueKey);
        var issue = _service.IssueGetById(outwardIssueKey);
        Assert.IsTrue(issue.Fields.Issuelinks.Any(il => il.InwardIssue.Key == inwardIssueKey && il.Type.Outward == linkType));

        var linkId = issue.Fields.Issuelinks.First(il => il.InwardIssue.Key == inwardIssueKey && il.Type.Outward == linkType).Id;
        var issuelink = _service.IssueLinkGetById(linkId);
        Assert.IsNotNull(issuelink);
        Assert.AreEqual(linkId, issuelink.Id);
    }
}
