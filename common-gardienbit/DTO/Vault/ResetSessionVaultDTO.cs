namespace common_gardienbit.DTO.Vault
{
    public class ResetSessionVaultDTO
    {
        public required LoginSuccessVaultDTO successDTO { get; set; }
        public required KeyVaultDTO keyVaultDTO { get; set; }
    }
}
