
namespace api_gardienbit.Models
{
    public class VaultUserLink : IDataModel
    {
        public Guid Id { get; set; }
        public Guid VaultId { get; set; }
        public Vault? Vault { get; set; }

        public Guid UserId { get; set; }
        public Client? User { get; set; }

        public DateTime AccessGrantedAt { get; set; }
    }

}
