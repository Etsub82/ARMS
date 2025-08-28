using Application.Contracts.IRepository;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class ApplicationRepository : GenericRepository<ApplicationModel>, IApplicationRepository
    {
        private readonly ARMSDbContext _dbContext;

        public ApplicationRepository(ARMSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApplicationModel> GetApplicationByCredentials(string appId, string appKey)
        {
            return (await _dbContext.ApplicationModels
                .Include(a => a.ApplicationGroup)
                    .ThenInclude(ag => ag.GroupRoles)
                        .ThenInclude(gr => gr.RoleModel)
                .FirstOrDefaultAsync(a => a.AppId == appId && a.AppKey == appKey))!;
        }
    }
}
