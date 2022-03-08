namespace JiraChangesNotifier.Jira
{
    public partial class JiraClient
    {
        private class JiraSearchRequestData
        {
            public string jql { get; set; }
            public string[] fields { get; set; }
            public string[] expand { get; set; }
        }
    }
}
