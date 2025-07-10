using System.ComponentModel.DataAnnotations;

namespace api_gardienbit.Models
{
    public class Client : IDataModel
    {
        #region Attributes
        [Key]
        public Guid CliId { get; set; }
        public required string CliName { get; set; }
        public Guid CliEntraId { get; set; }
        public ICollection<Vault> CliVaults { get; set; } = [];
        public ICollection<VaultSession> CliSessions { get; set; } = [];

        #endregion
    }
}