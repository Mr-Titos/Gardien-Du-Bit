using api_gardienbit.DAL;

namespace api_gardienbit.Repositories
{
    public class CategoryRepository(EFContext context)
        : ARepository<Models.Category>(context, context.Categories)
    {
        public override async Task<Models.Category?> UpdateObject(Guid id, Models.Category updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.CatName = updatedObj.CatName;
            existingObj.CatVault = updatedObj.CatVault;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }
        public IEnumerable<Models.Category> GetCategoriesByVault(Guid vauId)
        {
            List<Models.Category> objs = this.contextMain.Where(f => f!.CatVault!.VauId.Equals(vauId)).ToList();
            objs.ForEach(p => contextMain.Entry(p).References.ToList().ForEach(r => r.Load())); // Needed to load all links of the object to other tables
            return objs;
        }
    }
}