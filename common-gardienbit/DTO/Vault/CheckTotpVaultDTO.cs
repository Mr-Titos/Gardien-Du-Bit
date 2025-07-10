namespace common_gardienbit.DTO.Vault
{
    [Serializable]
    public class CheckTotpVaultDTO
    {
        public Guid vauId { get; set; }
        public required string token { get; set; }

        public object Serialize() => new { vauId, token };
    }
}