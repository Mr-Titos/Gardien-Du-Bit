using api_gardienbit.DAL;
using api_gardienbit.Models;

namespace api_gardienbit.Repositories
{
    public class VaultUserLinkRepository(EFContext context) : ARepository<Models.VaultUserLink>(context ?? throw new ArgumentNullException(nameof(context)), context.VaultUserLinks)
    {
        public override Task<VaultUserLink?> UpdateObject(Guid id, VaultUserLink updatedUser)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> DeleteVaultUserLink(Guid vaultId, Guid userId)
        {
            var entity = await context.VaultUserLinks.FindAsync(vaultId, userId);
            if (entity != null)
            {
                context.VaultUserLinks.Remove(entity);
                await context.SaveChangesAsync();
            }
            return true;
        }
    }
}
