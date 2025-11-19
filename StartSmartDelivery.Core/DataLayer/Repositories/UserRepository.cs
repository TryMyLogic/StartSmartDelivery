using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Models;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly PasswordHasher<string> _hasher;
        public UserRepository(ApplicationDbContext context, PasswordHasher<string>? hasher = null) : base(context)
        {
            _hasher = hasher ?? new();
        }

        public async Task<bool> AuthenticateUser(string username, string password)
        {
            User? user = await GetQueryable().Where(u => u.Username == username).FirstOrDefaultAsync();
            if (user is null) return false;
            PasswordVerificationResult result = _hasher.VerifyHashedPassword(username, user.Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
