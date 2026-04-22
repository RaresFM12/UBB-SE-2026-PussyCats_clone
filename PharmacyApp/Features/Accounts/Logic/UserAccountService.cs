using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Features.Accounts.Logic
{
    public class UserAccountService : IUserAccountService
    {
        private const string IdSearchPrefix = "id:";
        private const string UsernameSearchPrefix = "username:";
        private const string EmailSearchPrefix = "mail:";

        public User? CurrentUser { get; private set; }

        public IUsersRepository usersRepository { get; private set; }

        public UserAccountService(IUsersRepository usersRepository)
        {
            CurrentUser = null;
            this.usersRepository = usersRepository;
        }

        public void Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("E-mail cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.");
            }

            if (!UserValidationService.isCorrectEmailFormat(email))
            {
                throw new Exception("Not a valid e-mail");
            }

            try
            {
                User foundUser = usersRepository.GetUserByEmail(email);

                if (foundUser.IsDisabled)
                {
                    throw new Exception("Account disabled");
                }

                if (!SecurityService.VerifyPassword(password, foundUser.PasswordHash))
                {
                    throw new Exception("Incorrect password");
                }

                CurrentUser = foundUser;
            }
            catch (ArgumentException)
            {
                throw new Exception("E-mail not found");
            }
        }

        public void Register(
            string email,
            string password,
            string confirmPassword,
            string username,
            string phoneNumber)
        {
            if (!UserValidationService.isCorrectEmailFormat(email))
            {
                throw new Exception("Not a valid email format\nmust be <text>@<text>.<text>");
            }

            if (!UserValidationService.isCorrectPasswordFormat(password))
            {
                throw new Exception("Incorrect format, must have: min 8 chars\n -1 symbol from {!,@,#,%,^,*}\n -1 capital and 1 small letter\n -1 digit");
            }

            if (password != confirmPassword)
            {
                throw new Exception("Passwords don't match");
            }

            try
            {
                User user = usersRepository.GetUserByEmail(email);
                throw new Exception("Email already linked to an account");
            }
            catch (ArgumentException)
            {
            }

            string hashedPassword = SecurityService.HashPassword(password);
            bool discountNotificationsSetting = false;
            usersRepository.AddUser(email, phoneNumber, hashedPassword, username, discountNotificationsSetting);
            CurrentUser = usersRepository.GetUserByEmail(email);
        }

        public void UpdateProfile(string newUsername, string newPhoneNumber)
        {
            if (CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (string.IsNullOrEmpty(newUsername))
            {
                newUsername = CurrentUser.Email.Split("@")[0];
            }
            else if (!UserValidationService.isCorrectUsernameFormat(newUsername))
            {
                throw new Exception("Invalid new username");
            }

            if (!string.IsNullOrEmpty(newPhoneNumber) &&
                !UserValidationService.isCorrectPhoneNumberFormat(newPhoneNumber))
            {
                throw new Exception("Invalid new phone number");
            }

            newPhoneNumber = string.IsNullOrEmpty(newPhoneNumber)
                ? CurrentUser.PhoneNumber
                : newPhoneNumber;

            CurrentUser.PhoneNumber = newPhoneNumber;
            CurrentUser.Username = newUsername;
            usersRepository.UpdateUser(CurrentUser);
        }

        public void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!SecurityService.VerifyPassword(oldPassword, CurrentUser.PasswordHash))
            {
                throw new Exception("Incorrect password");
            }

            if (!UserValidationService.isCorrectPasswordFormat(newPassword))
            {
                throw new Exception("New password must comply with the rules");
            }

            if (newPassword != confirmNewPassword)
            {
                throw new Exception("Passwords don't match");
            }

            string newPasswordHash = SecurityService.HashPassword(newPassword);

            CurrentUser.PasswordHash = newPasswordHash;
            usersRepository.UpdateUser(CurrentUser);
        }

        public List<User> SearchUsers(string query)
        {
            if (CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!CurrentUser.IsAdmin)
            {
                throw new Exception($"Current user with id={CurrentUser.Id} not an admin");
            }

            query = query.Trim();
            List<User> queriedUsers = usersRepository.GetAllUsers();

            if (query.StartsWith(IdSearchPrefix))
            {
                try
                {
                    int id = int.Parse(query.Substring(IdSearchPrefix.Length));
                    return queriedUsers.Where(user => user.Id == id).ToList();
                }
                catch (FormatException)
                {
                }
            }

            if (query.StartsWith(UsernameSearchPrefix))
            {
                string username = query.Substring(UsernameSearchPrefix.Length);
                return queriedUsers.Where(user => user.Username.Contains(username)).ToList();
            }

            if (query.StartsWith(EmailSearchPrefix))
            {
                string mail = query.Substring(EmailSearchPrefix.Length);
                return queriedUsers.Where(user => user.Email.Contains(mail)).ToList();
            }

            return queriedUsers;
        }

        public void PromoteToAdmin(User client)
        {
            if (CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!CurrentUser.IsAdmin)
            {
                throw new Exception($"Current user with id={CurrentUser.Id} not an admin");
            }

            if (client.IsAdmin || client.IsDisabled)
            {
                return;
            }

            client.IsAdmin = true;
            usersRepository.UpdateUser(client);
        }

        public void DisableAccount(User client)
        {
            if (CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!CurrentUser.IsAdmin)
            {
                throw new Exception($"Current user with id={CurrentUser.Id} not an admin");
            }

            if (client.IsAdmin || client.IsDisabled)
            {
                return;
            }

            client.IsDisabled = true;
            usersRepository.UpdateUser(client);
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}