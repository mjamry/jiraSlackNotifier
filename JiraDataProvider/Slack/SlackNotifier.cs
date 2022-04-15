using JiraDataProvider;
using SlackBotMessages;
using SlackBotMessages.Models;
using System.Collections.Generic;
using System.Linq;

namespace JiraChangesNotifier.Slack
{
    public class SlackNotifier
    {
        private readonly SlackConfig _config;
        Dictionary<string, SbmClient> _clients;

        public SlackNotifier(SlackConfig config)
        {
            _config = config;
            _clients = new Dictionary<string, SbmClient>();
            foreach(var p in _config.ProjectWeebhooks)
            {
                _clients.Add(p.Item1, new SbmClient(p.Item2));
            }
        }

        public void SendJiraUpdate(Dictionary<string, IEnumerable<IssueDto>> data)
        {
            foreach (var project in data.Keys)
            {
                if (data[project].Count() > 0)
                {
                    foreach (var issue in data[project])
                    {
                        var title = $"[ {issue.Key} ] " + (issue.IsNew ? "New Issue" : "Issue update");
                        var message = new Message(title);
                        var attachment = new Attachment()
                            .AddField("Poject", project, true)
                            .AddField("Issue", $"{new SlackLink(issue.Url, issue.Key)}", true);

                        if (issue.IsNew)
                        {
                            attachment
                                .AddField("Author", issue.Reporter, true)
                                .AddField("Created", issue.Updated.ToString(_config.DateTimeFormat), true)
                                .AddField("Description", issue.Description);
                        }

                        foreach (var c in issue.Changes.OrderByDescending(i => i.Created))
                        {
                            attachment
                                .AddField("Author", c.Author, true)
                                .AddField("Created", c.Created.ToString(_config.DateTimeFormat), true)
                                .AddField(c.Field, c.Content);
                        }

                        message.AddAttachment(attachment);
                        _clients[project].Send(message);
                    }
                }
            }
        }
    }
}
