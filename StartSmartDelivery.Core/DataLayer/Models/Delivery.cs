using System.ComponentModel.DataAnnotations;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int TaskId { get; set; }

        [Required]
        public required int DriverId { get; set; }

        [Required]
        public required int VehicleId { get; set; }

        [Required]
        public required DateTimeOffset DispatchTime { get; set; }
    }
}
