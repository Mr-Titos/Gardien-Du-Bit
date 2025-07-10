using api_gardienbit.DAL;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    public class PwdPackageRepository(EFContext context) : ARepository<Models.PwdPackage>(context, context.PwdPackages)
    {
        public override async Task<Models.PwdPackage?> UpdateObject(Guid id, Models.PwdPackage updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.PwpName = updatedObj.PwpName;
            existingObj.PwpContent = updatedObj.PwpContent;
            existingObj.PwpUrl = updatedObj.PwpUrl;
            existingObj.PwpCom = updatedObj.PwpCom;
            existingObj.PwpLastUpdateDate = DateTime.UtcNow;
            existingObj.PwpCategory = updatedObj.PwpCategory;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }

        public bool ObjectExist(Guid id)
        {
            return contextSql.PwdPackages.Any(e => e.PwpId == id);
        }

        public async Task<List<Models.PwdPackage>> GetObjectsFromVault(Guid vaultId)
        {
            return await contextSql.PwdPackages
                .Where(p => p.PwPVault.VauId == vaultId)
                .Include(p => p.PwPVault)
                .ThenInclude(p => p.VauCategories)
                .ToListAsync();
        }
    }
}
