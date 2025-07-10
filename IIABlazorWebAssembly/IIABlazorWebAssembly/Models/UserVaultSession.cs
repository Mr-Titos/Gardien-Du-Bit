using common_gardienbit.DTO.Vault;

namespace IIABlazorWebAssembly.Models
{
    public class UserVaultSession
    {
        public UserVaultSession()
        {
            this.usrVauSessionId = Guid.NewGuid();
        }

        public Guid usrVauSessionId { get; set; }
        public LoginSuccessVaultDTO? successDTO { get; set; }
        public byte[]? DerivedEncryptionKey { get; set; }

        public string? totpData { get; set; }

    }
}
