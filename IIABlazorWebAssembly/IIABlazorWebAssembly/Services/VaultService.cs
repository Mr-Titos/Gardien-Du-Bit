using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Models;

namespace IIABlazorWebAssembly.Services
{
    public class VaultService
    {
        private readonly EncryptionService encryptionService;
        public bool isShared { get; set; }

        public VaultService(EncryptionService encryptionService)
        {
            this.encryptionService = encryptionService;
        }

        public async Task<(UserVaultSession?, KeyVaultDTO?)> LoginVaultFromLocalStorage(Guid vaultId)
        {
            // Check if the user is already logged in the back
            var alreadyLoggedIn = await encryptionService.LoginFromLocalStorage(vaultId);
            if (alreadyLoggedIn.Item1 != null && alreadyLoggedIn.Item2 != null)
                return (alreadyLoggedIn.Item1, alreadyLoggedIn.Item2);

            return (null, null);
        }

        public async Task<(UserVaultSession?, KeyVaultDTO?)> LoginVault(Guid vaultId, string clearPassword, bool isTotp)
        {
            var alreadyLoggedIn = await LoginVaultFromLocalStorage(vaultId);
            if (alreadyLoggedIn.Item1 != null && alreadyLoggedIn.Item2 != null)
                return (alreadyLoggedIn.Item1, alreadyLoggedIn.Item2);

            try
            {
                // Login to the Vault and getting the public key of the session
                (UserVaultSession?, KeyVaultDTO?) tupleLogin = await encryptionService.LoginVaultProcedure(clearPassword, vaultId, isTotp);
                return (tupleLogin.Item1, tupleLogin.Item2);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        public async Task AttemptVaultLogin(Guid vaultId, string password, bool isTotp, Func<Guid, Task> onSuccess)
        {
            var (session, keyVault) = await this.LoginVault(vaultId, password, isTotp);
            if (session == null || (keyVault == null && isTotp == false))
            {
                throw new InvalidDataException();
            }
            else
            {
                await onSuccess(vaultId); // Appelle l'action spécifique à faire après le login
            }
        }
    }
}
