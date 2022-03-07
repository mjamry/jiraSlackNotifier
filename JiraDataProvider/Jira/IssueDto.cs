using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraDataProvider
{
    public class IssueDto
    {
        public IssueDto()
        {
            IsNew = false;
            Changes = Enumerable.Empty<ChangeDto>();
        }

        public string Key { get; set; }
        public string Assignee { get; set; }
        public DateTime Updated { get; set; }
        public string Description { get; set; }
        public string Reporter { get; set; }
        public bool IsNew { get; set; }
        public string Priority { get; internal set; }
        public string Status { get; internal set; }
        public string Type { get; internal set; }
        public IEnumerable<ChangeDto> Changes { get; set; }
    }
}