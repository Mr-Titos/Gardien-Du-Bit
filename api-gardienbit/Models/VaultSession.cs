namespace api_gardienbit.Models
{
    public class VaultSession : IDataModel
    {
        public Guid VasId { get; set; }
        public required Vault VasVault { get; set; }
        public required Client VasClient { get; set; }
        public bool VasIsTotpResolved { get; set; } = false;
        public byte[] VasPrivateKey { get; set; } = [];
        public byte[] VasPublicKey { get; set; } = [];
        public byte[] VasEncryptionKeyPrivate { get; set; } = [];
        public byte[] VasEncryptionKeyPublic { get; set; } = [];
        public DateTime VasEntryDate { get; set; }
        public DateTime VasLastActivityDate { get; set; }
    }
}