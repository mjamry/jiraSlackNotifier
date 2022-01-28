using JiraDataProvider;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class JiraChangeProviderTests
    {
        private JiraConfig _config;
        private IJiraApiWrapper _api;
        private ILogger _logger;

        private List<JiraCommentDto> _comments;
        private List<JiraChangeLogDto> _changes;
        private List<JiraIssueDto> _issues;
        private List<JiraProjectDto> _projects;

        private const int TIME_PERIOD_IN_MINUTES = 5;

        [SetUp]
        public void Setup()
        {
            _config = new JiraConfig()
            {
                SupportedIssueFields = new[] { "testField1", "testField2" },
                SupportedProjectKeys = new[] { "P1", "P2", "P3", "P4" },
                TimePeriodForUpdatesInMinutes = -TIME_PERIOD_IN_MINUTES,
            };
            
            _api = Substitute.For<IJiraApiWrapper>();
            _logger = Substitute.For<ILogger>();

            _projects = new List<JiraProjectDto>()
            {
                new JiraProjectDto(){ Key = "P1" },
                new JiraProjectDto(){ Key = "P2" },
                new JiraProjectDto(){ Key = "P3" },
                new JiraProjectDto(){ Key = "P4" }
            }; 
            
            _issues = new List<JiraIssueDto>()
            {
                new JiraIssueDto(){ Key = "I1", Project = "P1", Updated = MinutesBeforeNow(1) },
                new JiraIssueDto(){ Key = "I2", Project = "P2", Updated = MinutesBeforeNow(1) },
                new JiraIssueDto(){ Key = "I3", Project = "P3", Updated = MinutesBeforeNow(1) },
                new JiraIssueDto(){ Key = "I4", Project = "P4", Updated = MinutesBeforeNow(1) },
            };

            _comments = new List<JiraCommentDto>()
            {
                new JiraCommentDto() { 
                    Author = "testCommentAuthor", 
                    Body = "testCommentContent", 
                    CreatedDate = MinutesBeforeNow(3)
                }
            };

            _changes = new List<JiraChangeLogDto>()
            {
                new JiraChangeLogDto() {
                    Author = new JiraUserDto() { Email = "testChangeEmail" },
                    Content="testChangeContent",
                    CreatedDate = MinutesBeforeNow(1),
                    Items = new List<JiraIssueChangeLogItem>()
                    {
                        new JiraIssueChangeLogItem() {
                            FieldName = "testField1",
                            FromValue = "fromValue1",
                            ToValue = "toValue1"
                        }
                    }
                }
            };

            _api.GetChangeLogsAsync(Arg.Any<string>()).Returns(_changes);
            _api.GetCommentsAsync(Arg.Any<string>()).Returns(_comments);
            _api.GetIssues().Returns(_issues);
            _api.GetProjectsAsync().Returns(_projects);
        }

        [Test]
        public async Task ShouldGetAllProjects()
        {
            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public async Task ShouldGetOneIssuePerProject()
        {
            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then
            
            foreach(var proj in result)
            {
                Assert.AreEqual(1, proj.Value.Count());
            }
        }

        [Test]
        public async Task ShouldGetIssuesForAProject()
        {
            //setup
            _issues.Add(new JiraIssueDto() { Key = "P1", Created = MinutesBeforeNow(2) });
            _comments.Add(new JiraCommentDto() { Author = "testAuthor", CreatedDate = MinutesBeforeNow(3) });

            _api.GetIssues().Returns(_issues);
            _api.GetCommentsAsync("P1").Returns(_comments);

            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then

            Assert.AreEqual(3, result["P1"].First().Changes.Count());
        }

        [Test]
        public async Task ShouldGetIssuesForAProject_WhenIssueHasSomeOldComments()
        {
            //setup
            _issues.Add(new JiraIssueDto() { Key = "II1", Project = "P1",  Updated = MinutesBeforeNow(2) });
            _comments.Add(new JiraCommentDto() { Author = "testAuthor", CreatedDate = MinutesBeforeNow(3) });
            _comments.Add(new JiraCommentDto() { Author = "testAuthor", CreatedDate = MinutesBeforeNow(6) });
            _comments.Add(new JiraCommentDto() { Author = "testAuthor", CreatedDate = MinutesBeforeNow(7) });

            _api.GetIssues().Returns(_issues);
            _api.GetCommentsAsync("P1").Returns(_comments);

            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then

            Assert.AreEqual(3, result["P1"].First().Changes.Count());
        }

        [Test]
        public async Task ShouldGetIssuesForAProject_WhenProjectHaveOldIssues()
        {
            //setup
            _issues.Add(new JiraIssueDto() { Key = "II1", Project = "P1", Updated = MinutesBeforeNow(3) });
            _issues.Add(new JiraIssueDto() { Key = "II2", Project = "P1", Updated = MinutesBeforeNow(4) });
            _issues.Add(new JiraIssueDto() { Key = "II3", Project = "P1", Updated = MinutesBeforeNow(5) });
            _issues.Add(new JiraIssueDto() { Key = "II4", Project = "P1", Updated = MinutesBeforeNow(6) });

            _api.GetIssues().Returns(_issues);

            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then

            Assert.AreEqual(3, result["P1"].Count());
        }

        [Test]
        public async Task ShouldGetIssuesForAProject_WhenLastUpdatedWasAlmostTooLongAgo()
        {
            //setup
            // Issue updated 4min 55sec ago (5min is configured)
            _issues.Add(new JiraIssueDto() { Key = "P1", Updated = MinutesBeforeNow(5).AddSeconds(-5) });
            _comments.Add(new JiraCommentDto() { Author = "testAuthor", CreatedDate = MinutesBeforeNow(3) });

            _api.GetIssues().Returns(_issues);
            _api.GetCommentsAsync("P1").Returns(_comments);

            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then

            Assert.AreEqual(3, result["P1"].First().Changes.Count());
        }

        [Test]
        public async Task ShouldGetCommentDetails()
        {
            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            
            //when
            var result = await provider.GetLatestChanges();
            
            //then
            var resultComment = result["P1"].First().Changes.First();
            Assert.AreEqual("testCommentAuthor", resultComment.Author);
            Assert.AreEqual("Comment", resultComment.Field);
            Assert.AreEqual("testCommentContent", resultComment.Content);
        }

        [Test]
        public async Task ShouldGetChangeDetails()
        {
            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);

            //when
            var result = await provider.GetLatestChanges();

            //then
            var resultComment = result["P1"].First().Changes.Last();
            Assert.AreEqual("testChangeEmail", resultComment.Author);
            Assert.AreEqual("testField1", resultComment.Field);
            Assert.AreEqual("fromValue1 -> toValue1", resultComment.Content);
        }

        [Test]
        public async Task ShouldGetOnlySupportedProjects()
        {
            _config.SupportedProjectKeys = new[] { "P1", "P2" };
            //given
            var provider = new JiraChangesProvider(_api, _config, _logger);
            //when
            var result = await provider.GetLatestChanges();
            //then
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task ShouldGetOnlySupportedFieldsFromIssues()
        {
            Assert.Fail();
        }



        private DateTime MinutesBeforeNow(int minutes)
        {
            return DateTime.Now.AddMinutes(-minutes);
        }
    }
}