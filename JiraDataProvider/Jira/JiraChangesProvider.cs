using Atlassian.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraDataProvider
{
    public class JiraChangesProvider
    {
        private readonly JiraConfig _config;
        private Jira _instance;

        public JiraChangesProvider(JiraConfig config)
        {
            _config = config;
        }

        public void Connect()
        {
            _instance = Jira.CreateRestClient(_config.Url, _config.User, _config.Password);
        }

        public async Task<Dictionary<string, IEnumerable<IssueDto>>> GetLatestChanges()
        {
            var projects = await GetProjects();

            var output = new Dictionary<string, IEnumerable<IssueDto>>();
            foreach (var p in projects)
            {
                var updatedIssues = await GetChangesForProject(p);
                if(updatedIssues.Count() > 0)
                {
                    output.Add(p, updatedIssues);
                }
            }

            return output;
        }

        private async Task<IEnumerable<string>> GetProjects()
        {
            var allProjects = await _instance.Projects.GetProjectsAsync();

            return allProjects
                .Where(p => _config.SupportedProjectKeys.Contains(p.Key))
                .Select(p => p.Key)
                .ToList();
        }

        private async Task<IEnumerable<IssueDto>> GetChangesForProject(string project)
        {
            var output = new List<IssueDto>();

            var projectIssues = _instance.Issues.Queryable
                .Where(i => i.Project == project)
                .Where(i => i.Updated > (DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes)))
                .OrderByDescending(i => i.Updated);

            foreach (var i in projectIssues)
            {
                var issue = new IssueDto()
                {
                    Assignee = i.Assignee,
                    Updated = i.Updated.Value,
                    Key = i.Key.Value,
                    Reporter = i.Reporter,
                    Description = i.Description
                };

                if (i.Created == i.Updated)
                {
                    issue.IsNew = true;
                    output.Add(issue);
                    break;
                }

                var comments = await GetIssueComments(i.Key.Value);
                var changes = await GetIssueChanges(i.Key.Value);

                issue.Changes = comments.Concat(changes);

                if(issue.Changes.Count() > 0)
                {
                    output.Add(issue);
                }
            }

            return output;
        }

        private async Task<IEnumerable<ChangeDto>> GetIssueComments(string issueKey)
        {
            var output = new List<ChangeDto>();
            var comments = await _instance.Issues.GetCommentsAsync(issueKey);

            foreach (var c in comments
              .Where(c => c.CreatedDate > (DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes)))
              .OrderByDescending(c => c.CreatedDate))
            {
                output.Add(new ChangeDto()
                {
                    Author = c.Author,
                    Created = c.CreatedDate.Value,
                    Field = "Comment",
                    Content = c.Body
                });
            }

            return output;
        }

        private async Task<IEnumerable<ChangeDto>> GetIssueChanges(string issueKey)
        {
            var output = new List<ChangeDto>();
            var changes = await _instance.Issues.GetChangeLogsAsync(issueKey);

            foreach (var c in changes
                .Where(c => c.CreatedDate > (DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes)))
                .OrderByDescending(c => c.CreatedDate))
            {
                foreach (var it in c.Items.Where(i => _config.SupportedIssueFields.Any(f => f == i.FieldName)))
                {
                    output.Add(new ChangeDto()
                    {
                        Author = c.Author.Email,
                        Created = c.CreatedDate,
                        Field = it.FieldName,
                        Content = $"{it.FromValue} -> {it.ToValue}"
                    });
                }
            }

            return output;
        }
    }
}
