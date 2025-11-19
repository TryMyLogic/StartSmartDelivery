using System.ComponentModel.DataAnnotations;
using StartSmartDelivery.Core.SharedLayer.Enums;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    public class DeliveryTask
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        public required string OrderNumber { get; set; }

        [Required]
        public required string CustomerCode { get; set; }

        [Required]
        public required string Name { get; set; }

        public string? Telephone { get; set; }

        [Required]
        public required string Cellphone { get; set; }

        public string? Email { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
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
