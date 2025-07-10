using api_gardienbit.Models;
using common_gardienbit.DTO.PwdPackage;

namespace api_gardienbit.Utils
{
    public class DTOMapperUtils
    {
        public static EncryptedField MapToEncryptedField(FieldPwdPackageDTO fieldPwdPackageDTO)
        {
            return new EncryptedField
            {
                EnfCipherText = Convert.FromBase64String(fieldPwdPackageDTO.EnfCipherTextBase64 ?? throw new ArgumentNullException()),
                EnfAuthTag = Convert.FromBase64String(fieldPwdPackageDTO.EnfAuthTagBase64 ?? throw new ArgumentNullException()),
                EnfInitialisationVector = Convert.FromBase64String(fieldPwdPackageDTO.EnfInitialisationVectorBase64 ?? throw new ArgumentNullException())
            };
        }

        public static FieldPwdPackageDTO MapToFieldPwdPackageDTO(EncryptedField encryptedField)
        {
            return new FieldPwdPackageDTO
            {
                EnfCipherTextBase64 = Convert.ToBase64String(encryptedField.EnfCipherText),
                EnfAuthTagBase64 = Convert.ToBase64String(encryptedField.EnfAuthTag),
                EnfInitialisationVectorBase64 = Convert.ToBase64String(encryptedField.EnfInitialisationVector)
            };
        }
    }
}
