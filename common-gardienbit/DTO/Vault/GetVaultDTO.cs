namespace common_gardienbit.DTO.Vault
{
    public class GetVaultDTO
    {
        public Guid vauId { get; set; }

        public required string vauName { get; set; }

        public Guid? cliId { get; set; }
        public bool isShared { get; set; }
        public bool isTotp { get; set; }
        public bool vauFavorite { get; set; }
    }
}
