using System.Collections.Generic;

namespace JiraChangesNotifier.Slack
{
    public class SlackConfig
    {
        public string DateTimeFormat { get; set; }

        public IEnumerable<(string, string)> ProjectWeebhooks { get; set; }
    }
}
