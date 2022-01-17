using JiraDataProvider;
using SlackBotMessages;
using SlackBotMessages.Models;
using System.Collections.Generic;
using System.Linq;

namespace JiraChangesNotifier.Slack
{
    public class SlackNotifier
    {
        Dictionary<string, SbmClient> _clients;

        public SlackNotifier(IEnumerable<(string, string)> projectWeebhooks)
        {
            _clients = new Dictionary<string, SbmClient>();
            foreach(var p in projectWeebhooks)
            {
                _clients.Add(p.Item1, new SbmClient(p.Item2));
            }
        }

        public void SendJiraUpdate(Dictionary<string, IEnumerable<IssueDto>> data)
        {
            foreach (var p in data.Keys)
            {
                if (data[p].Count() > 0)
                {
                    foreach (var i in data[p])
                    {
                        var title = $"[ {i.Key} ] " + (i.IsNew ? "New Issue" : "Issue update");
                        var message = new Message(title);
                        var attachment = new Attachment()
                            .AddField("Poject", p, true)
                            .AddField("Issue", i.Key, true);

                        if (i.IsNew)
                        {
                            attachment
                                .AddField("Author", i.Reporter, true)
                                .AddField("Created", i.Updated.ToString(), true)
                                .AddField("Description", i.Description);
                        }

                        foreach (var c in i.Changes.OrderByDescending(i => i.Created))
                        {
                            attachment
                                .AddField("Author", c.Author, true)
                                .AddField("Created", c.Created.ToString(), true)
                                .AddField(c.Field, c.Content);
                        }

                        message.AddAttachment(attachment);
                        _clients[p].Send(message);
                    }
                }
            }
        }
    }
}
