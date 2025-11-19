using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Models;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class DriverRepository : RepositoryBase<Driver>, IDriverRepository
    {
        public DriverRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
