using StartSmartDelivery.Core.DataLayer.Models;

namespace StartSmartDelivery.Core.DataLayer.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> AuthenticateUser(string username, string password);
    }
}
