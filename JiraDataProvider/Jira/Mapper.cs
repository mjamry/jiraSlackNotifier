using Atlassian.Jira;
using AutoMapper;
using JiraDataProvider;
using System.Collections.Generic;

namespace JiraChangesNotifier.Jira
{
    class Mapper
    {

        private IMapper _mapper;
        public Mapper()
        {
            var map = new MapperConfiguration(config => {
                config.CreateMap<Project, JiraProjectDto>();
                config.CreateMap<Issue, JiraIssueDto>();
                config.CreateMap<Comment, JiraCommentDto>();
                config.CreateMap<IssueChangeLog, JiraChangeLogDto>();
            });

            _mapper = map.CreateMapper();
        }

        public IEnumerable<K> MapCollection<T, K>(IEnumerable<T> source)
        {
            return _mapper.Map<IEnumerable<T>, IEnumerable<K>>(source);
        }


    }
}
