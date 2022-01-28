using System;

namespace JiraDataProvider
{
    public class JiraCommentDto
    {
        public DateTime CreatedDate { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
    }
}
