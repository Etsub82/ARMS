using Domain;
using System.Threading.Tasks;

namespace Application.Contracts.IRepository
{
    public interface IApplicationRepository : IGenericRepository<ApplicationModel>
    {
        Task<ApplicationModel> GetApplicationByCredentials(string appId, string appKey);
    }
}
