using Microsoft.EntityFrameworkCore;
using PaymentCVSTS.Repositories.Basic;
using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Repositories
{
    public class UserAccountRepository : GenericRepository<UserAccount>
    {
        public UserAccountRepository() { }

        public async Task<UserAccount> GetUserAccount(string userName, string password)
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserName == userName && u.Password == password && u.IsActive == true);
            //return await _context.UserAccounts.FirstOrDefaultAsync(x => x.Email == userName && x.Password == password && u.IsActive == true);
            //return await _context.UserAccounts.FirstOrDefaultAsync(x => x.Phone == userName && x.Password == password && u.IsActive == true);
            //return await _context.UserAccounts.FirstOrDefaultAsync(x => x.UserName == Employee && x.Password == password && u.IsActive == true);
        }
    }
}
