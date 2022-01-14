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
        [FunctionName("JiraChangesFunction")]
        public static async Task Run([TimerTrigger("0 */5 * * * *"
            #if DEBUG
                , RunOnStartup=true
            #endif
            )]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Start function execution at: {DateTime.Now}");

            var config = new JiraConfig()
            {
                SupportedIssueFields = Environment.GetEnvironmentVariable("SupportedIssueFields").Split(','),
                SupportedProjectKeys = Environment.GetEnvironmentVariable("SupportedProjectKeys").Split(','),
                User = Environment.GetEnvironmentVariable("User"),
                Password = Environment.GetEnvironmentVariable("Password"),
                Url = Environment.GetEnvironmentVariable("Url"),
                UpdateTimeoutInMinutes = -int.Parse(Environment.GetEnvironmentVariable("UpdateTimeoutInMinutes"))
            };

            var provider = new JiraChangesProvider(config);
            provider.Connect();
            var changes = await provider.GetLatestChanges();

            if (changes.Count() > 0)
            {
                var slackBot = new SlackNotifier(Environment.GetEnvironmentVariable("SlackWebhookUrl"));
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
