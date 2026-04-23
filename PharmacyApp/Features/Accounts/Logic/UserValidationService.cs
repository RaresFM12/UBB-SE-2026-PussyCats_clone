using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Accounts.Logic
{
    public class UserValidationService
    {
        private const string EmailPattern =
            @"^.+@.+\..+";

        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*])[A-Za-z\d!@#%^*]{8,}$";

        private const string PhoneNumberPattern =
            @"^[0-9]+$";

        private const string UsernamePattern =
            @"^[A-Za-z_]+$";
        public static bool IsCorrectEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Regex.IsMatch(email.Trim(), EmailPattern);
        }
        public static bool IsCorrectPasswordFormat(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return Regex.IsMatch(password, PasswordPattern);
        }

        public static bool IsCorrectPhoneNumberFormat(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            return Regex.IsMatch(phoneNumber.Trim(), PhoneNumberPattern);
        }

        public static bool IsCorrectUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return Regex.IsMatch(username.Trim(), UsernamePattern);
        }
    }
}
