using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        void Logout();
    }
}
