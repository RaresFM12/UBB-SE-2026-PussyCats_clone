using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Features.Accounts.Logic
{
    public static class ServiceWrapper
    {
        public static UserAccountService UserAccountService { get; private set; }

        public static void Initialize()
        {
            IUsersRepository userRepository = new SQLUsersRepository();
            ISecurityService securityService = new SecurityService();
            IUserValidationService validationService = new UserValidationService();

            UserAccountService = new UserAccountService(userRepository, securityService, validationService);
        }
    }
}
