# jiraSlackNotifier
It is very simple but useful tool that let you know about updates on your Jira.
It works as an Azure Function triggered by a timer.

Here is an examplary setting json (which can be used localy): 

```json
{
  "Values": {
    "TimerInterval": "0 */5 * * * *",
    "TimePeriodForUpdatesInMinutes": "5",
    "IssueFields": "assignee,status,priority,type", // fields for which you want to have updates. Comments are included by default.
    "ProjectKeys": "projectKey1, projectKey2", // projects keys for you want to have updates. It is useful when you wiltiple projects on the same server.
    "ProjectWebhooks": "webhookUrl1, webhookUrl2", // Slack webhooks for which you want to get updates. Each webhook corresponds to each project.
    "User": "", // Jira user.
    "Password": "", // Jira password.
    "Url": "", // Jira server Url
  }
}
```
