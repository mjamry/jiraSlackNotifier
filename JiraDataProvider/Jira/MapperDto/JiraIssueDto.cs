using System;
using System.Collections.Generic;

namespace JiraDataProvider
{
    public class JiraIssueDto
    {
        public string Project { get; set; }
        public string Key { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Created { get; set; }
        public string Assignee { get; set; }
        public string Description { get; set; }
        public string Reporter { get; set; }
        public bool IsNew { get; set; }
        public IEnumerable<ChangeDto> Changes { get; set; }
    }
}
