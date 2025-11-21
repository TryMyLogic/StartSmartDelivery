using System.ComponentModel.DataAnnotations;
using StartSmartDelivery.Core.SharedLayer.Enums;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    public class DeliveryTask
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string OrderNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public required string CustomerCode { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Phone]
        [MaxLength(15)]
        public string? Telephone { get; set; }

        [Required]
        [Phone]
        [MaxLength(15)]
        public required string Cellphone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Address { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Product { get; set; }

        [Required]
        public required decimal Amount { get; set; }

        [Required]
        public required PaymentMethod PaymentMethod { get; set; }

        public string? Notes { get; set; }

        [Required]
        public DateTimeOffset ReceivedTimestamp { get; set; }
    }
}
