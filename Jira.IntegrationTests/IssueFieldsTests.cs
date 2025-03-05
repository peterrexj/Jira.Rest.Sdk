using NUnit.Framework;
using Pj.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.IntegrationTests
{
    internal class IssueFieldsTests : TestBase
    {
        [TestCase]
        public void Should_Get_Changelog()
        {
            var changelogs = _service.IssueChangelogsGet(EnvironmentVariables.IssueKey);
            Assert.IsNotNull(changelogs);
            Assert.Greater(changelogs.Count, 0);
        }

        [TestCase]
        public void Should_Update_Issue_Description()
        {
            var issueKey = "";
            var issuePreCheckDescription = _service.IssueGetById(issueKey).Fields.Description;

            var newDescription = "New description for the issue";
            _service.IssueDescriptionUpdate(issueKey, newDescription);
            var issue = _service.IssueGetById(issueKey);
            Assert.AreEqual(newDescription, issue.Fields.Description);
        }

        [TestCase]
        public void Should_Add_Labels_To_Issue()
        {
            if (EnvironmentVariables.Label.Count == 0)
            {
                Assert.Inconclusive("No versions to test with");
            }

            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueLabelAdd(issueKey, true, null, EnvironmentVariables.Label.ToArray());
            var issue = _service.IssueGetById(issueKey);
            CollectionAssert.IsSubsetOf(EnvironmentVariables.Label, issue.Fields.Labels);
        }

        [TestCase]
        public void Should_Update_Issue_Components()
        {
            if (EnvironmentVariables.Component.Count == 0)
            {
                Assert.Inconclusive("No versions to test with");
            }

            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueComponentUpdate(EnvironmentVariables.Component, true, issueKey);
            var issue = _service.IssueGetById(issueKey);
            CollectionAssert.IsSubsetOf(EnvironmentVariables.Component, issue.Fields.Components.Select(c => c.Name).ToList());
        }

        [TestCase]
        public void Should_Update_Affected_Versions()
        {
            if (EnvironmentVariables.Version.Count == 0)
            {
                Assert.Inconclusive("No versions to test with");
            }

            Should_Remove_Affected_Versions();
            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueAffectedVersionUpdate(EnvironmentVariables.Version, true, issueKey);
            var issue = _service.IssueGetById(issueKey);
            Assert.IsTrue(issue.Fields.Versions.Count >= EnvironmentVariables.Version.Count);
        }

        [TestCase]
        public void Should_Remove_Affected_Versions()
        {
            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueAffectedVersionRemove(issueKey);
            var issue = _service.IssueGetById(issueKey);
            Assert.IsTrue(issue.Fields.Versions.Count == 0);
        }

        [TestCase]
        public void Should_Update_Fix_Versions()
        {
            if (EnvironmentVariables.FixVersion.Count == 0)
            {
                Assert.Inconclusive("No versions to test with");
            }

            Should_Remove_Fix_Versions();
            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueFixVersionUpdate(EnvironmentVariables.FixVersion, true, issueKey);
            var issue = _service.IssueGetById(issueKey);
            Assert.IsTrue(issue.Fields.FixVersion.Count >= EnvironmentVariables.FixVersion.Count);
        }

        [TestCase]
        public void Should_Remove_Fix_Versions()
        {
            var issueKey = EnvironmentVariables.IssueKey;
            _service.IssueFixVersionRemove(issueKey);
            var issue = _service.IssueGetById(issueKey);
            Assert.IsTrue(issue.Fields.FixVersion.Count == 0);
        }

        [TestCase]
        public void Should_Get_Versions_In_Project()
        {
            var versions = _service.VersionsGet(EnvironmentVariables.ProjectKey);
            Assert.IsNotNull(versions);
            Assert.Greater(versions.Count, 0);
        }

        [TestCase]
        public void Should_Get_Version_By_Id()
        {
            var versionId = _service.VersionsGet(EnvironmentVariables.ProjectKey).FirstOrDefault().Id;
            if (versionId.IsEmpty())
            {
                Assert.Inconclusive("No versions to test with");
            }

            var version = _service.VersionGet(versionId);
            Assert.IsNotNull(version);
            Assert.AreEqual(versionId, version.Id);
        }

        [TestCase]
        public void Should_Get_Component_By_Project()
        {
            var components = _service.ComponentsGet(EnvironmentVariables.ProjectKey);
            Assert.IsNotNull(components);
            Assert.Greater(components.Count, 0);
        }

        [TestCase]
        public void Should_Get_Component_By_Id()
        {
            var componentId = _service.ComponentsGet(EnvironmentVariables.ProjectKey).FirstOrDefault().Id;
            if (componentId.IsEmpty())
            {
                Assert.Inconclusive("No components to test with");
            }
            var component = _service.ComponentGet(componentId);
            Assert.IsNotNull(component);
            Assert.AreEqual(componentId, component.Id);
        }
    }
}
