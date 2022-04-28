using JiraDataProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraChangesNotifier.Jira
{
    public partial class JiraResponseHandler
    {
        private readonly JiraConfig _config;
        private readonly ILogger _log;

        public JiraResponseHandler(JiraConfig config, ILogger log)
        {
            _config = config;
            _log = log;
        }

        public string GetNamedField(JToken source, string fieldName)
        {
            return source[JiraResponseFields.Fields][fieldName][JiraResponseFields.Name].ToString();
        }

        public T GetTypedField<T>(JToken source, string fieldName)
        {
            return (T)Convert.ChangeType(source[JiraResponseFields.Fields][fieldName], typeof(T));
        }

        public IEnumerable<ChangeDto> GetFieldsUpdate(JToken source)
        {
            var output = new List<ChangeDto>();
            try
            {
                foreach (var history in source[JiraResponseFields.Changelog][JiraResponseFields.Histories])
                {
                    foreach (var change in history[JiraResponseFields.Items])
                    {
                        var fieldName = change[JiraResponseFields.Field].ToString();
                        if (_config.SupportedIssueFields.Any(f => f == fieldName))
                        {
                            var changeTime = DateTime.Parse(history[JiraResponseFields.Created].ToString());
                            if(changeTime > DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes))
                            {
                                output.Add(new ChangeDto()
                                {
                                    Author = history[JiraResponseFields.Author][JiraResponseFields.Name].ToString(),
                                    Created = changeTime,
                                    Field = fieldName,
                                    Content = $"{change[JiraResponseFields.From]} -> {change[JiraResponseFields.To]}"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return output;
        }

        public IEnumerable<ChangeDto> GetCommentsUpdate(JToken source)
        {
            var output = new List<ChangeDto>();
            try
            {
                foreach (var comment in source[JiraResponseFields.Fields][JiraResponseFields.Comment][JiraResponseFields.Comments])
                {
                    var updated = DateTime.Parse(comment[JiraResponseFields.Updated].ToString());
                    if (updated > DateTime.Now.AddMinutes(_config.TimePeriodForUpdatesInMinutes))
                    {
                        output.Add(new ChangeDto()
                        {
                            Author = comment[JiraResponseFields.Author][JiraResponseFields.Name].ToString(),
                            Created = updated,
                            Field = "comment",
                            Content = comment[JiraResponseFields.Body].ToString()
                        });
                    }
                }
            }
            catch (Exception) { }
            return output;
        }
    }
}
