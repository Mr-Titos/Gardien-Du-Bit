namespace common_gardienbit.DTO.PwdPackage
{
    public class CreatePwdPackageDTO
    {
        public required FieldPwdPackageDTO pwpName { get; set; }
        public required FieldPwdPackageDTO pwpContent { get; set; }
        public required FieldPwdPackageDTO pwpUrl { get; set; }
        public required FieldPwdPackageDTO pwpCom { get; set; }
        public Guid vauId { get; set; }
        public string encryptedAuth { get; set; } = "";

        public Guid catId { get; set; }
    }
}