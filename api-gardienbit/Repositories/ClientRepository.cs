using api_gardienbit.DAL;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Repositories
{
    public class ClientRepository(EFContext context) : ARepository<Models.Client>(context, context.Clients)
    {
        public override async Task<Models.Client?> UpdateObject(Guid id, Models.Client updatedObj)
        {
            var existingObj = await contextMain.FindAsync(id);

            if (existingObj == null)
                return null;

            return existingObj;
        }

        public async Task<Models.Client?> GetObjectByExternalId(Guid externalId)
        {
            return await contextMain.FirstOrDefaultAsync(x => x.CliEntraId == externalId);
        }

    }
}
