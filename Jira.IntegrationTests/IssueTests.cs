using Jira.Rest.Sdk;
using NUnit.Framework;
using Pj.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.IntegrationTests
{
    internal class IssueTests : TestBase
    {

        [TestCase]
        public void Should_Filter_Issues()
        {
            Assert.IsNotEmpty(EnvironmentVariables.IssueFilter, "The value of issue filter is required");
            var issues = _service.IssueSearch(EnvironmentVariables.IssueFilter);
            Assert.IsNotNull(issues);
            Assert.Greater(issues.Count, 0, $"There should be at least one issue returned from the server by this filter [{EnvironmentVariables.IssueFilter}]");
        }

        [TestCase]
        public void Should_Create_Issue()
        {
            var issueSummary = $"{EnvironmentVariables.IssueNamePrefix}_{DateTimeEx.GetDateTimeReadable()}";
            var issue= _service.IssueCreate(EnvironmentVariables.ProjectKey, "Story", issueSummary,
                "Generated from Integration Tests", "High");
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

        //[TestCase]
        //public void Should_Find_Project()
        //{
        //    Assert.IsNotEmpty(EnvironmentVariables.ProjectKey, "The value of project key");
        //    var projects = service.ProjectsGetByNameOrKey(EnvironmentVariables.ProjectKey);
        //    Assert.IsNotNull(projects);
        //    Assert.AreEqual(projects.Count(), 1, "There should be at least one project returned from the server");
        //    Assert.AreEqual(projects[0].Key, EnvironmentVariables.ProjectKey, "The project key does not match with the filtered result");
        //}
    }
}
