using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class IssueMetadata
    {
        public string Name { get; set; }
        public IssueMetadataDetail Metadata { get; set; }
    }
}
