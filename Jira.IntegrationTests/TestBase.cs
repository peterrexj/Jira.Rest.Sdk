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
    public class TestBase
    {
        protected JiraService service = null;

        [SetUp]
        public void Setup()
        {
            EnvironmentVariables.SetEnvironmentContent(PjUtility.Runtime.LoadAssembly("Jira.IntegrationTests")
                .GetEmbeddedResourceAsText("Jira.IntegrationTests.Ex.EnvironmentVariableNames.data"));
            service = new JiraService(EnvironmentVariables.JiraServerUrl, EnvironmentVariables.JiraUsername, EnvironmentVariables.JiraPassword, isCloudVersion: false);
        }
    }
}
