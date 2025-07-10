using api_gardienbit.Models;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.DAL
{
    public class EFContext : DbContext
    {
        #region Constructor
        public EFContext()
        {

        }

        public EFContext(DbContextOptions<EFContext> options) :
            base(options)
        {

        }
        #endregion


        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Vault> Vaults { get; set; }
        public DbSet<PwdPackage> PwdPackages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogAction> LogActions { get; set; }
        public DbSet<VaultSession> VaultSessions { get; set; }
        public DbSet<VaultUserAccess> VaultUserAccess { get; set; }
        public DbSet<VaultUserLink> VaultUserLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurer les clés primaires
            modelBuilder.Entity<Client>().HasKey(c => c.CliId);
            modelBuilder.Entity<Vault>().HasKey(v => v.VauId);
            modelBuilder.Entity<PwdPackage>().HasKey(p => p.PwpId);
            modelBuilder.Entity<Category>().HasKey(f => f.CatId);
            modelBuilder.Entity<Log>().HasKey(l => l.LogId);
            modelBuilder.Entity<LogAction>().HasKey(la => la.LoaId);
            modelBuilder.Entity<VaultSession>().HasKey(vs => vs.VasId);
            modelBuilder.Entity<VaultUserAccess>().HasKey(vs => vs.VaultId);
            modelBuilder.Entity<VaultUserLink>().HasKey(vs => vs.VaultId);


            // Configurer les relations et les contraintes
            modelBuilder.Entity<Client>()
                .HasMany(c => c.CliVaults)
                .WithOne(v => v.VauClient)
                .IsRequired();

            modelBuilder.Entity<Vault>()
                .HasMany(v => v.VauPwdPackages)
                .WithOne(p => p.PwPVault);

            modelBuilder.Entity<Category>()
                       .HasOne(f => f.CatVault)
                       .WithMany(v => v.VauCategories)
                       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LogAction>()
                .HasMany(la => la.Logs)
                .WithOne(l => l.LogAction);

            modelBuilder.Entity<VaultSession>()
                .HasOne(vs => vs.VasVault)
                .WithMany(v => v.VauSessions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VaultSession>()
                .HasOne(vs => vs.VasClient)
                .WithMany(c => c.CliSessions)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VaultUserAccess>()
                .HasKey(v => new { v.VaultId, v.UserId });

            modelBuilder.Entity<VaultUserAccess>()
                .HasOne(v => v.Vault)
                .WithMany(v => v.SharedWithUsers)
                .HasForeignKey(v => v.VaultId);
            modelBuilder.Entity<VaultUserLink>()
                .HasKey(vul => new { vul.VaultId, vul.UserId });

            modelBuilder.Entity<VaultUserLink>()
                .HasOne(vul => vul.Vault)
                .WithMany(v => v.VaultUserLinks)
                .HasForeignKey(vul => vul.VaultId)
                .OnDelete(DeleteBehavior.Restrict);
            ;

            modelBuilder.Entity<PwdPackage>(entity =>
            {
                entity.OwnsOne(p => p.PwpName);
                entity.OwnsOne(p => p.PwpContent);
                entity.OwnsOne(p => p.PwpUrl);
                entity.OwnsOne(p => p.PwpCom);
            });
        }
    }
}
