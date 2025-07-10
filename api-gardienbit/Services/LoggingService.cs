using api_gardienbit.Models;
using api_gardienbit.Repositories;
using common_gardienbit.DTO.Exception;

namespace api_gardienbit.Services
{
    public class LoggingService(IServiceScopeFactory scopeFactory, UserIdentifierService userIdentifierService)
    {
        #region Properties
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly UserIdentifierService userIdentifierService = userIdentifierService;

        #endregion

        #region Methods

        public async Task LogAsync(string actionName, Vault? vault = null, Guid? pwdPackageId = null)
        {
            using var scope = scopeFactory.CreateScope();
            var laRepo = scope.ServiceProvider.GetRequiredService<LogActionRepository>();
            var logRepo = scope.ServiceProvider.GetRequiredService<LogRepository>();


            LogAction action = await laRepo.GetObjectByName(actionName) ?? throw new EntityNotFoundException<LogAction>();

            var user = userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>();

            var log = new Log
            {
                LogId = Guid.NewGuid(),
                LogCliEntraId = user.CliEntraId.ToString(),
                LogCliId = user.CliId.ToString(),
                LogVauId = vault?.VauId.ToString() ?? string.Empty,
                LogVauName = vault?.VauName ?? string.Empty,
                LogPwpId = pwdPackageId?.ToString() ?? string.Empty,
                LogAction = action,
                LogEntryDate = DateTime.UtcNow
            };

            await logRepo.CreateObject(log);
        }
        #endregion
    }
}
