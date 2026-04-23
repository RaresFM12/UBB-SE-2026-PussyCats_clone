using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Features.Accounts.Logic
{
    public static class ServiceWrapper
    {
        public static UserAccountService UserAccountService { get; private set; }

        public static void Initialize()
        {
            IUsersRepository userRepo = new SQLUsersRepository();
            UserAccountService = new UserAccountService(userRepo);
        }
    }
}
