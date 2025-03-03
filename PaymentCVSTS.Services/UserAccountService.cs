using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Repositories;

namespace PaymentCVSTS.Services
{
    public class UserAccountService
    {
        private readonly UserAccountRepository _userAccountRepository;

        public UserAccountService()
        {

            _userAccountRepository = new();
        }

        public Task<UserAccount> Login(string userName, string password)
        {
            return _userAccountRepository.GetUserAccount(userName, password);


        }
    }
}
