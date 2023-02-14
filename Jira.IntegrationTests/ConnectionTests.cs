using Jira.Rest.Sdk;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.IntegrationTests
{
    internal class ConnectionTests : TestBase
    {
        [TestCase]
        public void Verify_ConnectionToJira()
        {
            _service = new JiraService(EnvironmentVariables.JiraServerUrl,
              serviceUsername: EnvironmentVariables.JiraUsername,
              servicePassword: EnvironmentVariables.JiraPassword,
              isCloudVersion: true);
            Assert.IsTrue(_service.CanConnect, "Connection to the Jira server failed");
        }

        [TestCase]
        public void Verify_ConnectionUsingToken()
        {
            Assert.IsTrue(_service.CanConnect, "Connection to the Jira server failed");
        }

        [TestCase]
        public void Verify_ConnectionUsingTokenWhenNoUsernameAndHavingPassword()
        {
            _service = new JiraService(EnvironmentVariables.JiraServerUrl,
               serviceUsername: "",
               servicePassword: EnvironmentVariables.JiraPassword,
               isCloudVersion: false,
               authToken: EnvironmentVariables.JiraAuthToken);
            Assert.IsTrue(_service.CanConnect, "Connection to the Jira server failed");
        }

    }
}
