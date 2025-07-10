using api_gardienbit.DAL;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    public class VaultSessionRepository(EFContext context) : ARepository<Models.VaultSession>(context, context.VaultSessions)
    {
        public override async Task<Models.VaultSession?> UpdateObject(Guid id, Models.VaultSession updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.VasEncryptionKeyPrivate = updatedObj.VasEncryptionKeyPrivate;
            existingObj.VasClient = updatedObj.VasClient;
            existingObj.VasVault = updatedObj.VasVault;
            existingObj.VasPrivateKey = updatedObj.VasPrivateKey;
            existingObj.VasEntryDate = updatedObj.VasEntryDate;
            existingObj.VasLastActivityDate = updatedObj.VasLastActivityDate;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }

        public async Task<Models.VaultSession?> GetVaultSessionByVaultAndClient(Guid vaultId, Models.Client client)
        {
            var existingObj = await contextMain.Where(vs => vs.VasVault.VauId == vaultId && vs.VasClient.CliId == client.CliId).FirstAsync();
            if (existingObj == null)
                return null;

            contextMain.Entry(existingObj).References.ToList().ForEach(r => r.Load()); // Needed to load all links of the object to other tables

            return existingObj;
        }
    }
}
