using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.DataLayer.DTOs
{
    public record DeliveriesDTO(
           int TaskID,
           string OrderNumber,
           string CustomerCode,
           string Name,
           string? Telephone,
           string Cellphone,
           string? Email,
           string Address,
           string Product,
           decimal Amount,
           string PaymentMethod,
           string? Notes,
           DateTime ReceivedTimestamp,
           DateTime? DispatchTimestamp,
           string AssignedDriver
       )
    {
        public DeliveriesDTO() : this(
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            null,
            string.Empty,
            null,
            string.Empty,
            string.Empty,
            0m,
            string.Empty,
            null,
            DateTime.MinValue,
            null,
            string.Empty
        )
        { }
    }
}
