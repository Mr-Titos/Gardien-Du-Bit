namespace common_gardienbit.DTO.Vault
{
    [Serializable]
    public class CreateVaultDTO
    {
        public required string vauName { get; set; }
        public required string vauHash { get; set; }
        public bool vauFavorite { get; set; }
        public required string vauSalt { get; set; }
        public bool vauTotp { get; set; } = false;

        public object Serialize() => new { vauName, vauHash, vauFavorite, vauSalt, vauTotp };
    }
}