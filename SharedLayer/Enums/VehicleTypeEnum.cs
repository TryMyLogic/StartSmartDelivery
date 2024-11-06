using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
