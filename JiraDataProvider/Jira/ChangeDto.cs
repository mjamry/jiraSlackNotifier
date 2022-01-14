using System;

namespace JiraDataProvider
{
    public class ChangeDto
    {
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string Field { get; set; }
        public string Content { get; set; }
    }
}