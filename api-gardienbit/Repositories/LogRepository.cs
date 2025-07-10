using api_gardienbit.DAL;

namespace api_gardienbit.Repositories
{
    public class LogRepository(EFContext context) : ARepository<Models.Log>(context, context.Logs)
    {
        public override async Task<Models.Log?> UpdateObject(Guid id, Models.Log updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.LogAction = updatedObj.LogAction;
            existingObj.LogVauName = updatedObj.LogVauName;
            existingObj.LogVauId = updatedObj.LogVauId;
            existingObj.LogPwpId = updatedObj.LogPwpId;
            existingObj.LogCliEntraId = updatedObj.LogCliEntraId;
            existingObj.LogCliId = updatedObj.LogCliId;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }
    }
}
