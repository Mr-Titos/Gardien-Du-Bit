namespace common_gardienbit.DTO.Vault
{
    public class KeyVaultDTO
    {
        public required byte[] vauPrivateKey { get; set; }
        public required byte[] vauPublicKey { get; set; }
    }
}