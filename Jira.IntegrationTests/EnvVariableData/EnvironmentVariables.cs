using Pj.Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.IntegrationTests
{
    public static class EnvironmentVariables
    {
        public static string JiraServerUrl => PjUtility.EnvironmentVariables.GetValue("JiraServerUrl");
        public static string JiraUsername => PjUtility.EnvironmentVariables.GetValue("JiraUsername");
        public static string JiraPassword => PjUtility.EnvironmentVariables.GetValue("JiraPassword");
        public static string JiraAuthToken => PjUtility.EnvironmentVariables.GetValue("JiraAuthToken");
        public static string ProjectKey => PjUtility.EnvironmentVariables.GetValue("ProjectKey");
        public static string IssueFilter => PjUtility.EnvironmentVariables.GetValue("IssueFilter");
        public static string IssueNamePrefix => PjUtility.EnvironmentVariables.GetValue("IssueNamePrefix");
        public static string IssueCleanFilter => PjUtility.EnvironmentVariables.GetValue("IssueCleanFilter");
        public static string IssueKey => PjUtility.EnvironmentVariables.GetValue("IssueKey");
        public static List<string> FixVersion => PjUtility.EnvironmentVariables.GetValue("FixVersion").SplitAndTrim(",").ToList() ?? new List<string>();
        public static List<string> Version => PjUtility.EnvironmentVariables.GetValue("Version").SplitAndTrim(",").ToList() ?? new List<string>();
        public static List<string> Component => PjUtility.EnvironmentVariables.GetValue("Component").SplitAndTrim(",").ToList() ?? new List<string>();
        public static List<string> Label => PjUtility.EnvironmentVariables.GetValue("Label").SplitAndTrim(",").ToList() ?? new List<string>();
    }
}
