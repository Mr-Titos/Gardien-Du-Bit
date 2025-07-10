using System.ComponentModel.DataAnnotations;

namespace common_gardienbit.DTO.Category
{
    public class UpdateCategoryDTO
    {
        [Required]
        [StringLength(50)]
        public required string catName { get; set; }
        public Guid vaultId { get; set; }

    }
}
