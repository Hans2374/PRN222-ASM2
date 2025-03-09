using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.Services.Implements
{
    public class UserAccountService : IUserAccountService
    {
        private readonly UserAccountRepository _userAccountRepository;

        public UserAccountService()
        {
            _userAccountRepository = new UserAccountRepository();
        }

        public async Task<UserAccount> Authenticate(string userName, string password)
        {
            return await _userAccountRepository.GetUserAccount(userName, password);
        }
    }
}