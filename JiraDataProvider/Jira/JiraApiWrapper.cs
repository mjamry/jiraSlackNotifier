using Atlassian.Jira;
using JiraChangesNotifier.Jira;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public class JiraApiWrapper : IJiraApiWrapper
    {
        private class JiraIssueFieldDto
        {
            public string Name { get; set; }
        }
        private class JiraIssueDto2
        {
            public string Description { get; set; }
            public JiraIssueFieldDto Status { get; set; }
            public JiraIssueFieldDto Assignee { get; set; }
            public JiraIssueFieldDto Updated { get; set; }
            public JiraIssueFieldDto Created { get; set; }
            public JiraIssueFieldDto Priority { get; set; }
        }


        private class JiraRequestData
        {
            public string jql { get; set; }
            public string[] fields { get; set; }
            public int maxResults { get; set; }
        }

        private Jira _instance;
        private Mapper _mapper;
        private HttpClient _client;

        public JiraApiWrapper(JiraConfig config)
        {
            _instance = Jira.CreateRestClient(config.Url, config.User, config.Password);
            _mapper = new Mapper();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "NjQxNTQ3ODQxMzc5Ojn21CmVAYQ7ecjT9RgIE1jYUNJh");
        }

        public async Task<IEnumerable<JiraProjectDto>> GetProjectsAsync()
        {
            var response = await _client.GetAsync("https://support.future-processing.com/rest/api/2/project");
            response.EnsureSuccessStatusCode();
            var output = await response.Content.ReadAsAsync<JiraProjectDto[]>();

            var o = output.Select(prop => prop.Key).Aggregate((k1, k2) => $"{k1} {k2}");
            System.Console.WriteLine(o);

            await GetIssuesForProject("SMHAS");

            return output;
        }

        public async Task<IEnumerable<JiraIssueDto>> GetIssuesForProject(string projectKey)
        {
            var data = new JiraRequestData()
            {
                jql = $"project={projectKey} AND updated > -6d",
                fields = new string[] { "status", "name"}
            };
            var response = await _client.PostAsync("https://support.future-processing.com/rest/api/2/search", new StringContent(JsonSerializer.Serialize(data), Encoding.ASCII, "application/json"));
            //response.EnsureSuccessStatusCode();

            var issueOuptut = await response.Content.ReadAsStringAsync();

            var rawIssues = JObject.Parse(issueOuptut)["issues"];
            foreach(var i in rawIssues)
            {
                var key = i["key"].ToString();
                var description = i["fields"]["description"].ToString();
                var status = i["fields"]["status"]["name"].ToString();
                var priority = i["fields"]["priority"]["name"].ToString();
                var assignee = i["fields"]["assignee"]["name"].ToString();
                var issueType = i["fields"]["issueType"]["name"].ToString();
            }
            System.Console.WriteLine(issueOuptut);

            return Enumerable.Empty<JiraIssueDto>();
        }

        public IEnumerable<JiraIssueDto> GetIssues()
        {

            var jiraResult = _instance.Issues.Queryable;
            return _mapper.MapCollection<Issue, JiraIssueDto>(jiraResult);
        }

        public async Task<IEnumerable<JiraCommentDto>> GetCommentsAsync(string issueKey)
        {
            var jiraResult = await _instance.Issues.GetCommentsAsync(issueKey);
            return _mapper.MapCollection<Comment, JiraCommentDto>(jiraResult);
        }

        public async Task<IEnumerable<JiraChangeLogDto>> GetChangeLogsAsync(string issueKey)
        {
            var jiraResult = await _instance.Issues.GetChangeLogsAsync(issueKey);
            return _mapper.MapCollection<IssueChangeLog, JiraChangeLogDto>(jiraResult);
        }
    }
}
