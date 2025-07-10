using api_gardienbit.DAL;
using api_gardienbit.Models;
using common_gardienbit.DTO.Exception;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    public class VaultRepository(EFContext context) : ARepository<Models.Vault>(context ?? throw new ArgumentNullException(nameof(context)), context.Vaults)
    {
        private readonly int tokenExpiredAt = 15; // minutes 

        public override async Task<Models.Vault?> UpdateObject(Guid id, Models.Vault updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.VauSalt = updatedObj.VauSalt;
            existingObj.VauName = updatedObj.VauName;
            existingObj.VauClient = updatedObj.VauClient;
            existingObj.VauFavorite = updatedObj.VauFavorite;
            existingObj.VauCategories = updatedObj.VauCategories;
            existingObj.VauPwdPackages = updatedObj.VauPwdPackages;
            existingObj.VauLastUpdateDate = DateTime.UtcNow;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }

        public async Task<List<Models.Vault>> GetVaultByUser(Guid userId)
        {
            var objs = await contextMain.Where(x => x.VauClient!.CliId == userId).ToListAsync();
            objs.ForEach(p => contextMain.Entry(p).References.ToList().ForEach(r => r.Load())); // Needed to load all links of the object to other tables
            return objs;
        }
        public async Task<List<Vault>> GetVaultsAccessibleByUser(Guid userId)
        {

            var ownedVaults = await contextMain
                .Where(v => v.VauClient!.CliId == userId)
                .ToListAsync();

            var sharedVaults = await context.VaultUserLinks
                .Where(a => a.UserId == userId)
                .Include(a => a.Vault)
                .Select(a => a.Vault)
                .ToListAsync();


            foreach (var v in sharedVaults)
            {
                if (v != null)
                    v.IsShared = true;
            }

            foreach (var v in ownedVaults)
            {
                v.IsShared = false;
            }


            return [.. ownedVaults.Concat(sharedVaults).DistinctBy(v => v!.VauId)];
        }
        public async Task<VaultUserAccess> SaveVaultShareTokenAsync(Guid vaultId, string token, Guid targetUserId, int nbUsed)
        {

            var vault = await contextMain
                .FirstOrDefaultAsync(v => v.VauId == vaultId);

            if (vault == null)
                throw new UnauthorizedAccessException("L'utilisateur n'est pas propriétaire du coffre.");

            var shareToken = new VaultUserAccess
            {
                Id = Guid.NewGuid(),
                VaultId = vaultId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                IsUsed = 0,
                NbUsed = nbUsed,
                UserId = targetUserId
            };

            context.VaultUserAccess.Add(shareToken);
            await context.SaveChangesAsync();
            return shareToken;
        }
        public async Task<VaultUserAccess> GetVaultShareTokenAsync(Guid vaultId)
        {

            var vaultToken = await context.VaultUserAccess
                .Include(v => v.Vault)
                .FirstOrDefaultAsync(v => v.VaultId == vaultId);

            if (vaultToken == null)
                throw new UnauthorizedAccessException("Une erreur est survenu.");
            if (DateTime.UtcNow > vaultToken.CreatedAt.AddMinutes(tokenExpiredAt))
            {
                context.VaultUserAccess.Remove(vaultToken);
                throw new VaultAccessTokenExpiredException();
            }


            return vaultToken;
        }
        public async Task<VaultUserAccess> AddUserToVaultFromToken(string token, Guid userId)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            var shareToken = await context.VaultUserAccess
                .Include(t => t.Vault).ThenInclude(v => v!.VauClient)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (userId == shareToken?.Vault?.VauClient?.CliId)
            {
                throw new UnauthorizedAccessException("L'utilisateur est déjà propriétaire du coffre.");
            }
            if (DateTime.UtcNow > shareToken?.CreatedAt.AddMinutes(tokenExpiredAt))
            {
                context.VaultUserAccess.Remove(shareToken);
                throw new VaultAccessTokenExpiredException();
            }
            if (shareToken == null || shareToken.Vault == null)
                throw new Exception("Une erreur est survenu");

            var alreadyMember = await context.VaultUserLinks
                .AnyAsync(l => l.VaultId == shareToken.VaultId && l.UserId == userId);

            if (alreadyMember)
                throw new UnauthorizedAccessException("L'utilisateur est déjà membre du coffre.");

            var link = new VaultUserLink
            {
                Id = Guid.NewGuid(),
                VaultId = shareToken.VaultId,
                UserId = userId,
                AccessGrantedAt = DateTime.UtcNow
            };

            context.VaultUserLinks.Add(link);

            if (shareToken.NbUsed != shareToken.IsUsed)
            {
                shareToken.IsUsed += 1;

            }
            else
            {
                context.VaultUserAccess.Remove(shareToken);
            }
            context.VaultUserAccess.Update(shareToken);

            await context.SaveChangesAsync();
            return shareToken;
        }
        public async Task<List<VaultUserLink>> GetVaultAccessibleByUsers(Guid vaultId)
        {
            if (vaultId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(vaultId));
            }
            var sharedVaults = await context.VaultUserLinks
                .Where(a => a.VaultId == vaultId)
                .Include(a => a.User)
                .ToListAsync();


            return sharedVaults;
        }
        public async Task<bool> DeleteVaultAsync(Guid vaultId)
        {
            var sessions = await context.VaultSessions.Where(s => s.VasVault.VauId == vaultId).ToListAsync();

            // Charger les catégories liées au Vault (en asynchrone)
            var categories = await context.Categories
                .Where(c => c.CatVault != null && c.CatVault.VauId == vaultId)
                .ToListAsync();

            // Supprimer les catégories
            context.Categories.RemoveRange(categories);

            // Trouver le Vault
            var vault = await context.Vaults.FindAsync(vaultId);
            if (vault == null)
                return false;

            // Supprimer le Vault
            context.Vaults.Remove(vault);

            // Enregistrer les changements dans la même instance du contexte
            await context.SaveChangesAsync();

            return true;
        }

    }
}
