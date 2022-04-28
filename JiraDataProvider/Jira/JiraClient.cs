using JiraDataProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraChangesNotifier.Jira
{
    public partial class JiraClient
    {
        private HttpClient _client;
        private readonly JiraConfig _config;
        private readonly ILogger _log;
        private readonly JiraResponseHandler _responseHandler;
        private readonly string[] IssueBaseFields = new string[] {
            IssueFields.Assignee,
            IssueFields.Created,
            IssueFields.Description,
            IssueFields.IssueType,
            IssueFields.Key,
            IssueFields.Priority,
            IssueFields.Status,
            IssueFields.Updated,
            IssueFields.Comments,
        };
        private readonly string[] IssueBaseExpand = new string[] { "changelog" };

        public JiraClient(JiraConfig config, ILogger log)
        {
            _config = config;
            _log = log;

            _responseHandler = new JiraResponseHandler(_config, _log);
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.Token);
        }
        public async Task<IEnumerable<IssueDto>> GetIssuesForProject(string projectKey)
        {
            var data = new JiraSearchRequestData()
            {
                jql = $"project={projectKey} AND updated > {_config.TimePeriodForUpdatesInMinutes}m",
                fields = IssueBaseFields,
                expand = IssueBaseExpand
            };

            var response = await _client.PostAsync(
                $"{_config.Url}/rest/api/2/search",
                new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.ASCII,
                    "application/json"
            ));

            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            var rawIssues = JObject.Parse(responseData)["issues"];

            List<IssueDto> issues = new List<IssueDto>();
            foreach (var i in rawIssues)
            {
                var created = _responseHandler.GetTypedField<DateTime>(i, IssueFields.Created);
                var updated = _responseHandler.GetTypedField<DateTime>(i, IssueFields.Updated);
                var isNew = created == updated;
                var key = i[IssueFields.Key].ToString();
                var changes = isNew 
                        ? Enumerable.Empty<ChangeDto>() 
                        : _responseHandler.GetFieldsUpdate(i)
                        .Concat(_responseHandler.GetCommentsUpdate(i));

                _log.LogInformation(isNew ? $"New issue {key}" : $"Updated issue {key} with {changes.Count()} changes");

                if (isNew || changes.Count() > 0)
                {
                    issues.Add(new IssueDto()
                    {
                        Key = key,
                        Description = _responseHandler.GetTypedField<string>(i, IssueFields.Description),
                        Priority = _responseHandler.GetNamedField(i, IssueFields.Priority),
                        IsNew = created == updated,
                        Updated = updated,
                        Assignee = _responseHandler.GetNamedField(i, IssueFields.Assignee),
                        Status = _responseHandler.GetNamedField(i, IssueFields.Status),
                        Type = _responseHandler.GetNamedField(i, IssueFields.IssueType),
                        Url = $"{_config.Url}/browse/{key}",
                        Changes = changes
                    });
                }
            }
            
            return issues;
        }
    }
}
