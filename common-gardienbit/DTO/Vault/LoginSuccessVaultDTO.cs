namespace common_gardienbit.DTO.Vault
{
    public class LoginSuccessVaultDTO
    {
        public Guid vauId { get; set; }
        public required byte[] vauSalt { get; set; }
        public required byte[] vauPKey { get; set; }
    }
}
