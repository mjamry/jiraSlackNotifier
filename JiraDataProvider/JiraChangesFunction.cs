using JiraChangesNotifier.Jira;
using JiraChangesNotifier.Slack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public static class JiraChangesFunction
    {
        const int DefaultTimePeriod = 5;

        [FunctionName("JiraChangesFunction")]
        public static async Task Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Start function execution at: {DateTime.Now}");

            var config = new JiraConfig()
            {
                SupportedIssueFields = Environment.GetEnvironmentVariable("IssueFields").Split(','),
                SupportedProjectKeys = Environment.GetEnvironmentVariable("ProjectKeys").Split(','),
                User = Environment.GetEnvironmentVariable("User"),
                Password = Environment.GetEnvironmentVariable("Password"),
                Url = Environment.GetEnvironmentVariable("Url"),
                TimePeriodForUpdatesInMinutes = int.Parse(Environment.GetEnvironmentVariable("TimePeriodForUpdatesInMinutes")) | DefaultTimePeriod,
            };

            var jiraClient = new JiraClient(config, log);
            var changes = new Dictionary<string, IEnumerable<IssueDto>>();
            foreach(var p in config.SupportedProjectKeys)
            {
                var issues = await jiraClient.GetIssuesForProject(p);
                changes.Add(p, issues);
            }

            if (changes.Count() > 0)
            {
                var webhooks = Environment.GetEnvironmentVariable("ProjectWebhooks").Split(',');

                var projectWebhooks = config.SupportedProjectKeys.Zip(webhooks);
                var dateTimeFormat = Environment.GetEnvironmentVariable("DateTimeFormat");

                var slackConfig = new SlackConfig() { DateTimeFormat = dateTimeFormat, ProjectWeebhooks = projectWebhooks };
                var slackBot = new SlackNotifier(slackConfig);
                slackBot.SendJiraUpdate(changes);
            }
            else
            {
                log.LogInformation("No changes");
            }

            log.LogInformation($"End function execution at: {DateTime.Now}");
        }
    }
}
