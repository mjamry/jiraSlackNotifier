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
    public class JiraClient
    {
        // TYPES
        private static class IssueFields
        {
            public static string Key = "key";
            public static string Description = "description";
            public static string Assignee = "assignee";
            public static string Status = "status";
            public static string IssueType = "issuetype";
            public static string Updated = "updated";
            public static string Created = "created";
            public static string Priority = "priority";
        }

        private class JiraSearchRequestData
        {
            public string jql { get; set; }
            public string[] fields { get; set; }
            public string[] expand { get; set; }
        }

        private class JiraCommentsRequestData
        {
            public string orderBy { get; set; }
            public string[] expand { get; set; }
        }

        private HttpClient _client;
        private readonly JiraConfig _config;
        private readonly ILogger _log;

        //CONSTS
        private readonly string[] IssueBaseFields = new string[] {
            IssueFields.Assignee,
            IssueFields.Created,
            IssueFields.Description,
            IssueFields.IssueType,
            IssueFields.Key,
            IssueFields.Priority,
            IssueFields.Status,
            IssueFields.Updated,
        };
        private readonly string[] IssueBaseExpand = new string[] { "changelog" };

        public JiraClient(JiraConfig config, ILogger log)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "NjQxNTQ3ODQxMzc5Ojn21CmVAYQ7ecjT9RgIE1jYUNJh");
            _config = config;
            _log = log;
        }
        public async Task<IEnumerable<IssueDto>> GetIssuesForProject(string projectKey)
        {
            var data = new JiraSearchRequestData()
            {
                jql = $"project={projectKey} AND updated > -{_config.TimePeriodForUpdatesInMinutes}m",
                fields = IssueBaseFields,
                expand = IssueBaseExpand
            };

            var response = await _client.PostAsync(
                "https://support.future-processing.com/rest/api/2/search",
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
                _log.LogDebug($"Issue {i[IssueFields.Key].ToString()}");
                var created = GetTypedField<DateTime>(i, IssueFields.Created);
                var updated = GetTypedField<DateTime>(i, IssueFields.Updated);

                issues.Add(new IssueDto()
                {
                    Key = i[IssueFields.Key].ToString(),
                    Description = GetTypedField<string>(i, IssueFields.Description),
                    Priority = GetNamedField(i, IssueFields.Priority),
                    IsNew = created == updated,
                    Updated = updated,
                    Assignee = GetNamedField(i, IssueFields.Assignee),
                    Status = GetNamedField(i, IssueFields.Status),
                    Type = GetNamedField(i, IssueFields.IssueType),
                    Changes = GetFieldsUpdate(i).Concat(GetCommentsUpdate(i))
                });
            }
            
            return issues;
        }

        //HELPER
        private string GetNamedField(JToken source, string fieldName)
        {
            return source["fields"][fieldName]["name"].ToString();
        }

        private T GetTypedField<T>(JToken source, string fieldName)
        {
            return (T)Convert.ChangeType(source["fields"][fieldName], typeof(T));
        }

        private IEnumerable<ChangeDto> GetFieldsUpdate(JToken source)
        {
            var output = new List<ChangeDto>();
            try
            {
                foreach (var history in source["changelog"]["histories"])
                {
                    foreach (var change in history["items"])
                    {
                        var fieldName = change["field"].ToString();
                        if (_config.SupportedIssueFields.Any(f => f == fieldName))
                        {

                            output.Add(new ChangeDto()
                            {
                                Author = history["author"]["name"].ToString(),
                                Created = DateTime.Parse(history["created"].ToString()),
                                Field = fieldName.ToUpper(),
                                Content = $"{change["fromString"]} -> {change["toString"]}"
                            });
                        }
                    }
                }
            }
            catch(Exception) { }

            _log.LogDebug($"Fields updated: {output.Count()}");
            return output;
        }

        private IEnumerable<ChangeDto> GetCommentsUpdate(JToken source)
        {
            var output = new List<ChangeDto>();
            try 
            {
                foreach (var comment in source["field"]["comment"]["comments"])
                {
                    var updated = DateTime.Parse(comment["updated"].ToString());
                    if (updated > DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes))
                    {
                        output.Add(new ChangeDto()
                        {
                            Author = comment["author"]["name"].ToString(),
                            Created = updated,
                            Field = "Comment",
                            Content = comment["body"].ToString()
                        });
                    }
                }
            }
            catch (Exception) { }

            _log.LogDebug($"Comments updated: {output.Count()}");
            return output;
        }
    }
}
