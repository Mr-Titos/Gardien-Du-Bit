using api_gardienbit.DAL;
using api_gardienbit.Models;
using common_gardienbit.Enum;
using System.Security.Cryptography;
using System.Text;

public class FixturesServices
{
    private readonly EFContext context;
    public FixturesServices(EFContext context)
    {
        this.context = context;
    }

    public void ResetDatabase()
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        Client client = new Client() { CliId = Guid.NewGuid(), CliEntraId = Guid.NewGuid(), CliName = "Client 1" };
        context.Clients.Add(client);
        var hashPwd = HashPassword("password");
        string oui = Convert.ToBase64String(hashPwd);
        Vault vault = new Vault() { VauFavorite = false, VauId = Guid.NewGuid(), VauEntryDate = new DateTime(), VauName = "Coffre 1", VauHash = hashPwd, VauSalt = GenerateSalt(), VauClient = client };
        context.Vaults.Add(vault);

        Category cat = new Category() { CatId = Guid.NewGuid(), CatName = "Catégorie 1", CatVault = vault };
        context.Categories.Add(cat);

        LogAction logA1 = new LogAction() { LoaId = Guid.NewGuid(), LoaName = LogActionName.Visualize };
        LogAction logA2 = new LogAction() { LoaId = Guid.NewGuid(), LoaName = LogActionName.Update };
        LogAction logA3 = new LogAction() { LoaId = Guid.NewGuid(), LoaName = LogActionName.Delete };
        LogAction logA4 = new LogAction() { LoaId = Guid.NewGuid(), LoaName = LogActionName.Create };
        LogAction logA5 = new LogAction() { LoaId = Guid.NewGuid(), LoaName = LogActionName.Share };

        Log log = new Log() { LogId = Guid.NewGuid(), LogCliEntraId = "Entreprise 1", LogCliId = client.CliId.ToString(), LogVauId = vault.VauId.ToString(), LogVauName = "Coffre 1", LogPwpId = String.Empty, LogEntryDate = new DateTime(), LogAction =  logA1};
        Log log2 = new Log() { LogId = Guid.NewGuid(), LogCliEntraId = "Entreprise 2", LogCliId = client.CliId.ToString(), LogVauId = vault.VauId.ToString(), LogVauName = "Coffre 2", LogPwpId = String.Empty, LogEntryDate = new DateTime(), LogAction = logA1 };
        Log log3 = new Log() { LogId = Guid.NewGuid(), LogCliEntraId = "Entreprise 2", LogCliId = Guid.NewGuid().ToString(), LogVauId = vault.VauId.ToString(), LogVauName = "Coffre 1", LogPwpId = String.Empty, LogEntryDate = new DateTime(), LogAction = logA1 };
        context.Logs.Add(log);
        context.Logs.Add(log2);
        context.Logs.Add(log3);
        context.LogActions.Add(logA1);
        context.LogActions.Add(logA2);
        context.LogActions.Add(logA3);
        context.LogActions.Add(logA4);
        context.LogActions.Add(logA5);


        context.SaveChanges();
    }
    public static byte[] HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }


}