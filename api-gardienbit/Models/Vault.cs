using System.ComponentModel.DataAnnotations.Schema;

namespace api_gardienbit.Models
{
    public class Vault : IDataModel
    {
        [NotMapped]
        public bool IsShared { get; set; }
        #region Attributes
        public Guid VauId { get; set; }

        public required string VauName { get; set; }

        public required byte[] VauHash { get; set; }

        public bool VauFavorite { get; set; }

        public required byte[] VauSalt { get; set; }

        public byte[]? VauTOTP { get; set; }

        public DateTime VauEntryDate { get; set; }

        public DateTime VauLastUpdateDate { get; set; }

        public Client? VauClient { get; set; }
        public Guid OwnerId { get; set; }

        public ICollection<PwdPackage> VauPwdPackages { get; set; } = [];
        public ICollection<Category> VauCategories { get; set; } = [];

        public ICollection<VaultSession> VauSessions { get; set; } = [];
        public ICollection<VaultUserAccess> SharedWithUsers { get; set; } = [];
        public ICollection<VaultUserLink> VaultUserLinks { get; set; } = [];


        #endregion
    }
}
