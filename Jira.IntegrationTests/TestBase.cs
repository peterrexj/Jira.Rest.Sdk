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
        protected JiraService? _service = null;

        [SetUp]
        public void Setup()
        {
            PjUtility.EnvironmentVariables.PathToEnvironmentVariableKeyNamesCollectionAssembly =
               IoHelper.CombinePath(PjUtility.Runtime.ExecutingFolder, "Jira.IntegrationTests.dll");
            PjUtility.EnvironmentVariables.PathToEnvironmentVariableKeyNamesCollectionFile = "Jira.IntegrationTests.EnvVariableData.EnvironmentVariableNames.data";
            _service = new JiraService(EnvironmentVariables.JiraServerUrl,
                EnvironmentVariables.JiraUsername, EnvironmentVariables.JiraPassword, isCloudVersion: true,
                jiraApiVersion: "3");
        }
    }
}
