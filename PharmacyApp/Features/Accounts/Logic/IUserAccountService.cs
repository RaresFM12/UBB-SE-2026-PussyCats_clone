using PharmacyApp.Models;
using System.Collections.Generic;

namespace PharmacyApp.Features.Accounts.Logic
{
    public interface IUserAccountService
    {
        User? CurrentUser { get; }

        void Login(string email, string password);

        void Register(
            string email,
            string password,
            string confirmPassword,
            string username,
            string phoneNumber);

        void UpdateProfile(string newUsername, string newPhoneNumber);

        void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword);

        List<User> SearchUsers(string query);

        void PromoteToAdmin(User client);

        void DisableAccount(User client);

        void Logout();
    }
}