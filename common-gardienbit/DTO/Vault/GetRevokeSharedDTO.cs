namespace common_gardienbit.DTO.Vault
{
    public class GetRevokeSharedDTO
    {
        public Guid id { get; set; }
        public required string userName { get; set; }
        public Guid userId { get; set; }
        public Guid vaultId { get; set; }
    }
}
