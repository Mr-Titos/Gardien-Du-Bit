namespace api_gardienbit.Models
{
    public class EncryptedField : IDataModel
    {
        public byte[] EnfCipherText { get; set; } = [];
        public byte[] EnfAuthTag { get; set; } = [];
        public byte[] EnfInitialisationVector { get; set; } = [];
    }
}
