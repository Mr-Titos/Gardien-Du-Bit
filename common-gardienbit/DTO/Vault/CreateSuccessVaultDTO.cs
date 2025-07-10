namespace common_gardienbit.DTO.Vault
{
    public class CreateSuccessVaultDTO
    {
        public Guid vauId { get; set; }
        public required string vauName { get; set; }
        public bool vauFavorite { get; set; }
        public Guid? cliId { get; set; }
        public bool isShared { get; set; }
        public string? qrCodeTotp { get; set; }
    }
}