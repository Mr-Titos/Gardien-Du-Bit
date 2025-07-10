using api_gardienbit.DAL;
using api_gardienbit.Models;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    public class LogActionRepository(EFContext context) : ARepository<Models.LogAction>(context, context.LogActions)
    {
        public override async Task<Models.LogAction?> UpdateObject(Guid id, Models.LogAction updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            existingObj.LoaName = updatedObj.LoaName;

            await contextSql.SaveChangesAsync();

            return existingObj;
        }

        public async Task<Models.LogAction?> GetObjectByName(string name)
        {
            try
            {
                var existingObj = await contextMain.Where(la => la.LoaName.ToLower().Equals(name.ToLower())).FirstAsync() ?? throw new Exception();

                return existingObj;
            } catch(Exception)
            {
                return null;
            }

        }
    }
}