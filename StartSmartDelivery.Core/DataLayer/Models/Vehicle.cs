using System.ComponentModel.DataAnnotations;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    public class Vehicle
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        public required string Make { get; set; }

        [Required]
        public required string Model { get; set; }

        [Required]
        public required int Year { get; set; }

        [Required]
        public required string NumberPlate { get; set; }

        [Required]
        public required bool Availability { get; set; }
    }
}
