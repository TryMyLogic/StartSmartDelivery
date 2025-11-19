using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class DeliveryTask : RepositoryBase<DeliveryTask>, IDeliveryTaskRepository
    {
        public DeliveryTask(ApplicationDbContext context) : base(context)
        {
        }
    }
}
