using Atlassian.Jira;
using Atlassian.Jira.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public interface IJiraApiWrapper
    {
        Task<IEnumerable<Project>> GetProjectsAsync();
        JiraQueryable<Issue> GetIssues();
        Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey);
        Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(string issueKey);
    }
}
