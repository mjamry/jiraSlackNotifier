using JiraChangesNotifier.Slack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
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
                TimePeriodForUpdatesInMinutes = -int.Parse(Environment.GetEnvironmentVariable("TimePeriodForUpdatesInMinutes")) | DefaultTimePeriod,
            };

            var jiraApi = new JiraApiWrapper(config);

            var provider = new JiraChangesProvider(jiraApi, config, log);
            var changes = await provider.GetLatestChanges();

            if (changes.Count() > 0)
            {
                var projects = Environment.GetEnvironmentVariable("ProjectKeys").Split(',');
                var webhooks = Environment.GetEnvironmentVariable("ProjectWebhooks").Split(',');

                var projectWebhooks = projects.Zip(webhooks);
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
