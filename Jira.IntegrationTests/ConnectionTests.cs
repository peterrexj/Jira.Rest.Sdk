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
            Assert.IsTrue(service.CanConnect, "Connection to the Jira server failed");
        }
    }
}
