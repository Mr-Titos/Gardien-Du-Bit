using api_gardienbit.DAL;
using api_gardienbit.Models;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    // Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance.
    // This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    // https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#avoiding-dbcontext-threading-issues
    public abstract class ARepository<T> where T : class, IDataModel
    {
        protected readonly EFContext contextSql;

        protected readonly DbSet<T> contextMain;

        abstract public Task<T?> UpdateObject(Guid id, T updatedUser);

        protected ARepository(EFContext contextSql, DbSet<T> contextMain)
        {
            this.contextMain = contextMain;
            this.contextSql = contextSql;
        }
        public async Task<T> CreateObject(T obj)
        {
            contextMain.Add(obj);
            await contextSql.SaveChangesAsync();
            return obj;
        }
        public IEnumerable<T> GetObjects()
        {
            List<T> objs = this.contextMain.ToList();
            objs.ForEach(p => contextMain.Entry(p).References.ToList().ForEach(r => r.Load())); // Needed to load all links of the object to other tables
            return objs;
        }

        public async Task<T?> GetObjectById(Guid id)
        {
            T? objects = await contextMain.FindAsync(id);
            if (objects != null)
                contextSql.Entry(objects).References.ToList().ForEach(r => r.Load());
            return objects;
        }
        public async Task<bool> DeleteObject(Guid id)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return false;

            contextMain.Remove(existingObj);
            await contextSql.SaveChangesAsync();

            return true;
        }

        public async Task BeginTransactionAsyncFromRepository()
        {
            await contextSql.Database.BeginTransactionAsync();
        }

        public async Task EndTransactionAsyncFromRepository()
        {
            await contextSql.Database.CommitTransactionAsync();
        }
    }
}