using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Services.Interfaces
{
    public interface IUserAccountService
    {
        Task<UserAccount> Authenticate(string userName, string password);
    }
}