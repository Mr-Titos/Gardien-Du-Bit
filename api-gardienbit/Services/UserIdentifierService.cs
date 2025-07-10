using api_gardienbit.Repositories;

namespace api_gardienbit.Services
{
    public class UserIdentifierService(ClientRepository clientRepository)
    {
        #region Properties
        public Models.Client? CurrentInternalUser { get; set; }

        #endregion

        #region Methods
        public async Task<Models.Client> GetInternalUserAsync(Guid externalUserId, string userName)
        {
            if (CurrentInternalUser == null)
            {
                await clientRepository.BeginTransactionAsyncFromRepository();
                CurrentInternalUser = await clientRepository.GetObjectByExternalId(externalUserId);
                if (CurrentInternalUser == null)
                    CurrentInternalUser = await CreateInternalUserIdAsync(externalUserId, userName);
                await clientRepository.EndTransactionAsyncFromRepository();
            }

            return CurrentInternalUser;
        }

        private async Task<Models.Client> CreateInternalUserIdAsync(Guid externalUserId, string userName)
        {
            return await clientRepository.CreateObject(new Models.Client()
            {
                CliEntraId = externalUserId,
                CliName = userName,
                CliVaults = new List<Models.Vault>(),
                CliId = new Guid()
            });
        }
        #endregion
    }
}
