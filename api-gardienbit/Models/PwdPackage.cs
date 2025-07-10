namespace api_gardienbit.Models
{
    public class PwdPackage : IDataModel
    {
        #region Attributes
        public Guid PwpId { get; set; }
        public required EncryptedField PwpName { get; set; }
        public required EncryptedField PwpContent { get; set; }
        public required EncryptedField PwpUrl { get; set; }
        public required EncryptedField PwpCom { get; set; }
        public DateTime PwpEntryDate { get; set; }
        public DateTime PwpLastUpdateDate { get; set; }
        public required Vault PwPVault { get; set; }
        public Category? PwpCategory { get; set; }
        #endregion

    }
}