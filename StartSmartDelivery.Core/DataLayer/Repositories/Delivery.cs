using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class Delivery : RepositoryBase<Delivery>, IDeliveryRepository
    {
        public Delivery(ApplicationDbContext context) : base(context)
        {
        }
    }
}
