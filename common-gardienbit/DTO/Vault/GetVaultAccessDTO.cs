namespace common_gardienbit.DTO.Vault
{
    public class GetVaultAccessDTO
    {
        public Guid id { get; set; }
        public Guid? vaultId { get; set; }
        public string? token { get; set; }
        public DateTime createdAt { get; set; }
        public int? isUsed { get; set; }
        public int? nbUsed { get; set; }
        public Guid userId { get; set; }
        public string? qrCodeTotp { get; set; }

    }
}
