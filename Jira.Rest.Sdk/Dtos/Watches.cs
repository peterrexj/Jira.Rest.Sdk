using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public partial class Watches
    {
        public Uri Self { get; set; }
        public long WatchCount { get; set; }
        public bool IsWatching { get; set; }
    }
}
