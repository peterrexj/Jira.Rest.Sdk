using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class UpdateIssueRequest
    {
        [JsonProperty("update", NullValueHandling = NullValueHandling.Ignore)]
        public Update Update { get; set; }
    }

    public class Update
    {
        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public List<dynamic> Description { get; set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public List<Component> Components { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public List<dynamic> AffectedVersions { get; set; }

        [JsonProperty("fixVersions", NullValueHandling = NullValueHandling.Ignore)]
        public List<dynamic> FixVersions { get; set; }

        [JsonProperty("assignee", NullValueHandling = NullValueHandling.Ignore)]
        public List<AssigneeOnUpdate> Assignee { get; set; }

        [JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
        public List<OperationJiraPropertySet> Labels { get; set; }
    }

    public class OperationJiraPropertyAdd
    {
        [JsonProperty("add", NullValueHandling = NullValueHandling.Ignore)]
        public string Add { get; set; }
    }

    public class OperationJiraPropertySet
    {
        [JsonProperty("set", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Set { get; set; }
    }
    public class AssigneeOnUpdate
    {
        [JsonProperty("set", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Set { get; set; }
    }
    public class FixVersions
    {
        [JsonProperty("add", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Add { get; set; }

        [JsonProperty("set", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Set { get; set; }

        [JsonProperty("remove", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Remove { get; set; }
    }
    public class Component
    {
        [JsonProperty("add", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Add { get; set; }

        [JsonProperty("set", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Set { get; set; }

        [JsonProperty("remove", NullValueHandling = NullValueHandling.Ignore)]
        public OperationToIssueField Remove { get; set; }
    }

    public class OperationToIssueField
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
