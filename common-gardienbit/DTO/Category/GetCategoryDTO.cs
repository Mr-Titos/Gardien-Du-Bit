namespace common_gardienbit.DTO.Category
{
    public class GetCategoryDTO
    {
        public Guid catId { get; set; }
        public required string catName { get; set; }
        public Guid catVauId { get; set; }

    }
}
