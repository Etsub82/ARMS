using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Application.Contracts.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);

        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression);

        //Task<T> GetByEcxId(string name);
        Task<T> Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        public Task<bool> Exists(int id);
        // Task<T> GetByEmpId(Guid Empid);

        void DetachEntity(T entity);
    }
}
