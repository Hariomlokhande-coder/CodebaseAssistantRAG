using CodebaseAssistant.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodebaseAssistant.Application.Interfaces;
using System.Threading.Tasks;

namespace CodebaseAssistant.Infrastructure.Services
{
    public class IndexingService : IIndexingService
    {
        private readonly CodebaseAssistantDbContext _dbContext;

        public IndexingService(
            CodebaseAssistantDbContext dbContext)
        {
            _dbContext = dbContext;


        }

        public async Task IndexRepositoryAsync(
            Guid repositoryId)
        {
            throw new NotImplementedException();
        }

    }
}
