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
        // pattern: <text>@<text>.<text>
        private const string EmailPattern =
            @"^.+@.+\..+";

        // ≥8 chars, ≥1 uppercase, ≥1 lowercase, ≥1 digit,
        // ≥1 special from {!@#%^*}, no other characters allowed
        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*])[A-Za-z\d!@#%^*]{8,}$";

        // digits only
        private const string PhoneNumberPattern =
            @"^[0-9]+$";

        // English letters and underscore only
        private const string UsernamePattern =
            @"^[A-Za-z_]+$";
        public static bool isCorrectEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            return Regex.IsMatch(email.Trim(), EmailPattern);
        }
        public static bool isCorrectPasswordFormat(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
           
            return Regex.IsMatch(password, PasswordPattern);
        }

        public static bool isCorrectPhoneNumberFormat(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            return Regex.IsMatch(phoneNumber.Trim(), PhoneNumberPattern);
        }

        public static bool isCorrectUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;
            return Regex.IsMatch(username.Trim(), UsernamePattern);
        }

    }
}
