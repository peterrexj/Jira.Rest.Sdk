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
        //public static string PathToLocalSettingsFile => Path.Combine(PjUtility.Runtime.ExecutingRepositoryRootFolder, "local-settings.json");
        //private static string _envContentAsString = string.Empty;
        //public static void SetEnvironmentContent(string content)
        //{
        //    _envContentAsString = content;
        //}

        //private static ConcurrentDictionary<string, string> _localSettings;
        //public static ConcurrentDictionary<string, string> LocalSettings
        //{
        //    get
        //    {
        //        if (_localSettings == null)
        //        {
        //            _localSettings = new ConcurrentDictionary<string, string>();
        //            if (File.Exists(PathToLocalSettingsFile))
        //            {
        //                _localSettings = Pj.Library.JsonHelper.ConvertComplexJsonDataToDictionary(File.ReadAllText(PathToLocalSettingsFile));
        //                bool hasUpdates = false;
        //                foreach (var item in EnvironmentVariableNames)
        //                {
        //                    if (!_localSettings.ContainsKey(item))
        //                    {
        //                        if (!hasUpdates)
        //                            hasUpdates = true;

        //                        _localSettings.AddOrUpdate(item, "");
        //                    }
        //                }
        //                if (hasUpdates)
        //                {
        //                    SerializationHelper.SerializeToJson(_localSettings, PathToLocalSettingsFile);
        //                }
        //            }
        //            else
        //            {
        //                DownloadEnvironmentVariablesLocalSettings();
        //            }
        //        }
        //        return _localSettings;
        //    }
        //}

        //private static List<string> _environmentVariableNames;
        //public static List<string> EnvironmentVariableNames =>
        //    _environmentVariableNames ?? (_environmentVariableNames = _envContentAsString
        //        .SplitAndTrim(Environment.NewLine)
        //        .SelectMany(s => s.SplitAndTrim("\n"))
        //        .Select(a => a.Trim())
        //        .Where(a => a.HasValue())
        //        .ToList());

        //public static string GetValue(string name)
        //{
        //    if (EnvironmentVariableNames.Contains(name))
        //    {
        //        if (LocalSettings.ReadData(name, throwErrorWhenNotFound: false, returnNullWhenNotFound: true)
        //            .HasValue())
        //        {
        //            return LocalSettings.ReadData(name, throwErrorWhenNotFound: false,
        //                returnNullWhenNotFound: true);
        //        }
        //        return Pj.Library.Helpers.WindowsOsHelper.GetEnvironmentVariable(name);
        //    }
        //    throw new Exception(@"You are trying to access an environment variable which is either not defined or not within the scope of the test.");
        //}

        //public static void DownloadEnvironmentVariablesLocalSettings()
        //{
        //    var store = (IDictionary<string, object>)new ExpandoObject();
        //    EnvironmentVariables.EnvironmentVariableNames.ForEach(x => store.Add(x, ""));
        //    SerializationHelper.SerializeToJson(store, PathToLocalSettingsFile);
        //}


        public static string JiraServerUrl => PjUtility.EnvironmentVariables.GetValue("JiraServerUrl");
        public static string JiraUsername => PjUtility.EnvironmentVariables.GetValue("JiraUsername");
        public static string JiraPassword => PjUtility.EnvironmentVariables.GetValue("JiraPassword");
        public static string ProjectKey => PjUtility.EnvironmentVariables.GetValue("ProjectKey");
        public static string IssueFilter => PjUtility.EnvironmentVariables.GetValue("IssueFilter");
        public static string IssueNamePrefix => PjUtility.EnvironmentVariables.GetValue("IssueNamePrefix");
        public static string IssueCleanFilter => PjUtility.EnvironmentVariables.GetValue("IssueCleanFilter");
    }
}
