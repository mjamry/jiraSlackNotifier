using Atlassian.Jira;
using JiraChangesNotifier.Jira;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public class JiraApiWrapper : IJiraApiWrapper
    {
        private Jira _instance;
        private Mapper _mapper;

        public JiraApiWrapper(JiraConfig config)
        {
            _instance = Jira.CreateRestClient(config.Url, config.User, config.Password);
            _mapper = new Mapper();
        }

        public async Task<IEnumerable<JiraProjectDto>> GetProjectsAsync()
        {
            var jiraResult = await _instance.Projects.GetProjectsAsync();
            return _mapper.MapCollection<Project, JiraProjectDto>(jiraResult);
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
