using JiraDataProvider;
using SlackBotMessages;
using SlackBotMessages.Models;
using System.Collections.Generic;
using System.Linq;

namespace JiraChangesNotifier.Slack
{
    public class SlackNotifier
    {
        private SbmClient _client;

        public SlackNotifier(string webhookUrl)
        {
            _client = new SbmClient(webhookUrl);
        }

        public void SendJiraUpdate(Dictionary<string, IEnumerable<IssueDto>> data)
        {
            foreach (var p in data.Keys)
            {
                if (data[p].Count() > 0)
                {
                    foreach (var i in data[p])
                    {
                        var message = new Message(i.IsNew ? $"New Issue {i.Key}" : $"Issue update {i.Key}");
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
                        _client.Send(message);
                    }
                }
            }
        }
    }
}
