namespace common_gardienbit.DTO.Vault
{
    [Serializable]
    public class CreateVaultAccessDTO
    {
        public Guid vaultId { get; set; }
        public required string token { get; set; }
        public DateTime createdAt { get; set; }
        public bool isUsed { get; set; }

        public Guid userId { get; set; }

        public object Serialize() => new { vaultId, token, createdAt, isUsed, userId };
    }
}
