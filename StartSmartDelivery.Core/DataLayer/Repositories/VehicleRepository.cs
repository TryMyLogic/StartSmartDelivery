using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Models;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class VehicleRepository : RepositoryBase<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
