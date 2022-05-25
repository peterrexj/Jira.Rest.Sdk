using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Fields
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public Status Status { get; set; }

        [JsonProperty("watcher", NullValueHandling = NullValueHandling.Ignore)]
        public Watcher Watcher { get; set; }

        [JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
        public List<Attachment> Attachment { get; set; }

        [JsonIgnore]
        public List<SubTask> SubTasks => SubTasksV1 ?? SubTasksV2;

        [JsonProperty("sub-tasks", NullValueHandling = NullValueHandling.Ignore)]
        public List<SubTask> SubTasksV1 { get; set; }

        [JsonProperty("subtasks", NullValueHandling = NullValueHandling.Ignore)]
        public List<SubTask> SubTasksV2 { get; set; }

        //public List<SubTask> Subtasks { get; set; }


        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("project", NullValueHandling = NullValueHandling.Ignore)]
        public Project Project { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public CommentList Comment { get; set; }

        [JsonProperty("issuelinks", NullValueHandling = NullValueHandling.Ignore)]
        public List<Issuelink> Issuelinks { get; set; }

        [JsonProperty("worklog", NullValueHandling = NullValueHandling.Ignore)]
        public WorklogList Worklog { get; set; }

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated { get; set; }

        [JsonProperty("timetracking", NullValueHandling = NullValueHandling.Ignore)]
        public Timetracking Timetracking { get; set; }

        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public Project Parent { get; set; }

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created { get; set; }

        [JsonProperty("assignee", NullValueHandling = NullValueHandling.Ignore)]
        public Author Assignee { get; set; }

        [JsonProperty("reporter", NullValueHandling = NullValueHandling.Ignore)]
        public Author Reporter { get; set; }

        [JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Labels { get; set; }

        [JsonProperty("issuetype", NullValueHandling = NullValueHandling.Ignore)]
        public IssueType IssueType { get; set; }

        [JsonProperty("fixVersions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Version> FixVersion { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Version> Versions { get; set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public List<ComponentList> Components { get; set; }


        [JsonProperty("lastViewed", NullValueHandling = NullValueHandling.Ignore)]
        public string LastViewed { get; set; }

        [JsonProperty("priority", NullValueHandling = NullValueHandling.Ignore)]
        public Priority Priority { get; set; }

        [JsonProperty("timeestimate", NullValueHandling = NullValueHandling.Ignore)]
        public object TimeEstimate { get; set; }

        [JsonProperty("aggregatetimeoriginalestimate", NullValueHandling = NullValueHandling.Ignore)]
        public object AggregateTimeOriginalEstimate { get; set; }

        [JsonProperty("aggregatetimeestimate", NullValueHandling = NullValueHandling.Ignore)]
        public object AggregateTimeEstimate { get; set; }

        [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
        public Assignee Creator { get; set; }

        [JsonProperty("aggregateprogress", NullValueHandling = NullValueHandling.Ignore)]
        public Progress AggregateProgress { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public Progress Progress { get; set; }

        [JsonProperty("votes", NullValueHandling = NullValueHandling.Ignore)]
        public Votes Votes { get; set; }

        [JsonProperty("timespent", NullValueHandling = NullValueHandling.Ignore)]
        public object TimeSpent { get; set; }

        [JsonProperty("aggregatetimespent", NullValueHandling = NullValueHandling.Ignore)]
        public object AggregateTimeSpent { get; set; }

        [JsonProperty("resolutiondate", NullValueHandling = NullValueHandling.Ignore)]
        public object ResolutionDate { get; set; }

        [JsonProperty("workratio", NullValueHandling = NullValueHandling.Ignore)]
        public long? WorkRatio { get; set; }

        [JsonProperty("watches", NullValueHandling = NullValueHandling.Ignore)]
        public Watches Watches { get; set; }

        [JsonProperty("timeoriginalestimate", NullValueHandling = NullValueHandling.Ignore)]
        public object TimeOriginalEstimate { get; set; }

        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public object Environment { get; set; }

        [JsonProperty("duedate", NullValueHandling = NullValueHandling.Ignore)]
        public object DueDate { get; set; }
    }
}
