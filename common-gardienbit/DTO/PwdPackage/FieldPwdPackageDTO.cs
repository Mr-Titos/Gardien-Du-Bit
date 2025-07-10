namespace common_gardienbit.DTO.PwdPackage
{
    public class FieldPwdPackageDTO
    {
        public string? EnfCipherTextBase64 { get; set; }
        public string? EnfAuthTagBase64 { get; set; }
        public string? EnfInitialisationVectorBase64 { get; set; }
        public string? EnfDecryptedText { get; set; }
    }
}