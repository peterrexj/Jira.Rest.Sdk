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
    internal class ProjectTests : TestBase
    {
        [TestCase]
        public void Verify_ListOfProjects()
        {
            var projects = _service?.ProjectsGet();
            Assert.IsNotNull(projects);
            Assert.Greater(projects?.Count, 0, "There should be at least one project returned from the server");
        }

        [TestCase]
        public void Should_Find_Project()
        {
            Assert.IsNotEmpty(EnvironmentVariables.ProjectKey, "The value of project key");
            var projects = _service?.ProjectsGetByNameOrKey(EnvironmentVariables.ProjectKey);
            Assert.IsNotNull(projects);
            Assert.AreEqual(projects?.Count, 1, "There should be at least one project returned from the server");
            Assert.AreEqual(projects?[0].Key, EnvironmentVariables.ProjectKey, "The project key does not match with the filtered result");
        }
    }
}
