using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public interface IJiraApiWrapper
    {
        Task<IEnumerable<JiraProjectDto>> GetProjectsAsync();
        IEnumerable<JiraIssueDto> GetIssues();
        Task<IEnumerable<JiraCommentDto>> GetCommentsAsync(string issueKey);
        Task<IEnumerable<JiraChangeLogDto>> GetChangeLogsAsync(string issueKey);
    }
}
