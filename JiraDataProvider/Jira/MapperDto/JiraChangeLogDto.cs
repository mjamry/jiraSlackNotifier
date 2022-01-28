using System;
using System.Collections.Generic;

namespace JiraDataProvider
{
    public class JiraChangeLogDto
    {
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; }
        public JiraUserDto Author { get; set; }
        public IEnumerable<JiraIssueChangeLogItem> Items { get; set; }
    }
}
