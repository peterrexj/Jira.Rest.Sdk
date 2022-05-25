using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Priority
    {
        public string Name { get; set; }
        public Uri Self { get; set; }
        public Uri IconUrl { get; set; }
        public long Id { get; set; }
        public string Description { get; set; }
    }
}
