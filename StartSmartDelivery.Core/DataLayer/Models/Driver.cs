using System.ComponentModel.DataAnnotations;
using StartSmartDelivery.Core.SharedLayer.Enums;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    // Maybe add : IEquatable<Driver> if used for comparisons in code
    public class Driver
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Surname { get; set; }

        [Required]
        [MaxLength(10)]
        public required string EmployeeNo { get; set; }

        [Required]
        public required LicenseType LicenseType { get; set; }
    }
}
