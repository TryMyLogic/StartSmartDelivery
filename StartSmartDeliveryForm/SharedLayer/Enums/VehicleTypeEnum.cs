using System.ComponentModel.DataAnnotations;

namespace StartSmartDeliveryForm.SharedLayer.Enums
{
    public enum VehicleTypeEnum
    {
        Dyna = 1,
        [Display(Name = "6m3Truck")] //VehicleTypeObject.GetDisplayName(); 
        CubeTruck,
        BigTruck
    }
}
