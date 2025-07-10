namespace common_gardienbit.DTO.PwdPackage
{
    public class DeletePwdPackageDTO
    {
        public Guid pwpId { get; set; }
        public required string enryptedAuth { get; set; }
    }
}