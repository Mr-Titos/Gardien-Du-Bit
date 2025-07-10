using common_gardienbit.DTO.Category;

namespace common_gardienbit.DTO.PwdPackage
{
    public class GetPwdPackageDTO
    {
        public Guid pwpId { get; set; }

        public required FieldPwdPackageDTO pwpName { get; set; }
        public required FieldPwdPackageDTO pwpContent { get; set; }
        public required FieldPwdPackageDTO pwpUrl { get; set; }
        public required FieldPwdPackageDTO pwpCom { get; set; }
        public required GetCategoryDTO pwpCategory { get; set; }

        public DateTime pwpEntryDate { get; set; }
        public DateTime pwpLastUpdateDate { get; set; }
    }
}
