# jiraSlackNotifier
It is very simple but useful tool that let you know about updates on your Jira Server.
It works as an Azure Function triggered by a timer.

Here is an examplary setting json (which can be used localy): 

```json
{
  "Values": {
    "TimerInterval": "0 */5 * * * *", // Azure function Timer configuration
    "TimePeriodForUpdatesInMinutes": "5",
    "IssueFields": "assignee,status,priority,type", // fields for which you want to have updates. Comments are included by default.
    "ProjectKeys": "projectKey1, projectKey2", // projects keys for you want to have updates. It is useful when you wiltiple projects on the same server.
    "ProjectWebhooks": "webhookUrl1, webhookUrl2", // Slack webhooks for which you want to get updates. Each webhook corresponds to each project.
    "Token": "", // Jira API token
    "Url": "", // Jira server Url
    "DateTimeFormat": "HH:mm dd.MM.yy" //DateTime format displayed in Slack messages
  }
}
```

Is uses https://github.com/prjseal/SlackBotMessages/ to send messages to the Slack channel.

Here you can find how to setup slack app/webhooks -> https://api.slack.com/messaging/webhooks

How to setup Azure timer -> https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions

How to add configuration to Azure function -> https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal
