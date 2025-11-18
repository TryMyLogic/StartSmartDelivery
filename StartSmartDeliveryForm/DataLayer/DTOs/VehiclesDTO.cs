namespace StartSmartDeliveryForm.DataLayer.DTOs
{
    public record VehiclesDTO(
        int VehicleID,
        string Make,
        string Model,
        int Year,
        string NumberPlate,
        int Availability
    )
    {
        public VehiclesDTO() : this(0, string.Empty, string.Empty, 0, string.Empty, 0) { }
    }
}
