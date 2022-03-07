using JiraDataProvider;
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

        private class JiraRequestData
        {
            public string jql { get; set; }
            public string[] fields { get; set; }
            public string[] expand { get; set; }
        }

        private HttpClient _client;
        private readonly JiraConfig _config;

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

        public JiraClient(JiraConfig config)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "NjQxNTQ3ODQxMzc5Ojn21CmVAYQ7ecjT9RgIE1jYUNJh");
            _config = config;
        }
        public async Task<IEnumerable<IssueDto>> GetIssuesForProject(string projectKey)
        {
            var data = new JiraRequestData()
            {
                jql = $"project={projectKey} AND updated > -6d",
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
            //response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();

            var rawIssues = JObject.Parse(responseData)["issues"];

            List<IssueDto> issues = new List<IssueDto>();
            foreach (var i in rawIssues)
            {
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
                    Changes = GetChanges(i)
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

        private IEnumerable<ChangeDto> GetChanges(JToken source)
        {
            //TODO get changes
            return Enumerable.Empty<ChangeDto>();
        }
    }
}
