using System;
using System.Collections.Generic;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public partial class Votes
    {
        public Uri Self { get; set; }
        public long VotesVotes { get; set; }
        public bool HasVoted { get; set; }
    }
}
