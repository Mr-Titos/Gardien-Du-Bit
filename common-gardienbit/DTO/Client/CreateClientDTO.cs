using System.ComponentModel.DataAnnotations;

namespace common_gardienbit.DTO.Client
{
    public class CreateClientDTO
    {
        [Required]
        public required string entraId { get; set; }
    }
}
