namespace api_gardienbit.Models
{
    public class VaultUserAccess
    {
        public Guid Id { get; set; }
        public Guid VaultId { get; set; }
        public required string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public int IsUsed { get; set; }
        public int NbUsed { get; set; }
        public Guid UserId { get; set; }
        public Vault? Vault { get; set; }


    }
}
