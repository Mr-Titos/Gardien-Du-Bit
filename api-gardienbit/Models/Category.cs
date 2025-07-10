namespace api_gardienbit.Models
{
    public class Category : IDataModel
    {
        #region Attributes
        public Guid CatId { get; set; }
        public required string CatName { get; set; }
        public DateTime CatEntryDate { get; set; }
        public Vault? CatVault { get; set; }
        public ICollection<PwdPackage>? CatPwdPackages { get; set; }
        #endregion
    }
}