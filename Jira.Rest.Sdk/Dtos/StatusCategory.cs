using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public partial class StatusCategory
    {
        public Uri Self { get; set; }
        public long Id { get; set; }
        public string Key { get; set; }
        public string ColorName { get; set; }
        public string Name { get; set; }
    }
}
