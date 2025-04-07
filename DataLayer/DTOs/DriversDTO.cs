using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.DataLayer.DTOs
{
    /*
    DTO:
    Encapsulates data to be transferred between different layers of an application (e.g., from data access to business logic).
    Reduces the number of method calls by bundling multiple data attributes into a single object.
    Improves performance by minimizing the amount of data that needs to be serialized/deserialized during communication.
    Promotes a clear separation between the data structure and business logic, enhancing maintainability and clarity.
    */

    public record DriversDTO(
      int DriverID,
      string Name,
      string Surname,
      string EmployeeNo,
      LicenseType LicenseType,
      bool Availability
    )
    {
        public DriversDTO() : this(0, string.Empty, string.Empty, string.Empty, default, false) { }
        public static readonly DriversDTO Empty = new(0, string.Empty, string.Empty, string.Empty, default, false);
    }

}
