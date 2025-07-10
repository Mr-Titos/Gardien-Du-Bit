using System.ComponentModel.DataAnnotations;

namespace common_gardienbit.DTO.Category
{
    public class CreateCategoryDTO
    {
        [Required]
        [StringLength(50)]
        public required string catName { get; set; }

        [Required]
        public Guid vaultId { get; set; }

    }
}
