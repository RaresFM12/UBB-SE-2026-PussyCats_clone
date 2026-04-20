using PharmacyApp.Models;

namespace PharmacyApp.Features.Accounts.Logic
{
    public class CurrentUserServiceAdapter : ICurrentUserService
    {
        public User CurrentUser => ServiceWrapper.UserAccountService.CurrentUser;
    }
}