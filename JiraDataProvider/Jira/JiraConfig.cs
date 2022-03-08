namespace JiraDataProvider
{
    public class JiraConfig
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string[] SupportedIssueFields { get; set; }
        public string[] SupportedProjectKeys { get; set; }
        public int TimePeriodForUpdatesInMinutes { get; set; }
    }
}