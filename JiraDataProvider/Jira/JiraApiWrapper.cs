using Atlassian.Jira;
using Atlassian.Jira.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public class JiraApiWrapper : IJiraApiWrapper
    {
        private Jira _instance;

        public JiraApiWrapper(JiraConfig config)
        {
            _instance = Jira.CreateRestClient(config.Url, config.User, config.Password);
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            return await _instance.Projects.GetProjectsAsync();
        }

        public JiraQueryable<Issue> GetIssues()
        {
            return _instance.Issues.Queryable;
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey)
        {
            return await _instance.Issues.GetCommentsAsync(issueKey);
        }

        public async Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(string issueKey)
        {
            return await _instance.Issues.GetChangeLogsAsync(issueKey);
        }
    }
}
