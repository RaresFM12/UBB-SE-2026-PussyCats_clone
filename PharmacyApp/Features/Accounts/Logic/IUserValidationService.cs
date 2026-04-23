using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Accounts.Logic
{
    public interface IUserValidationService
    {
        bool IsCorrectEmailFormat(string email);
        bool IsCorrectPasswordFormat(string password);
        bool IsCorrectPhoneNumberFormat(string phoneNumber);
        bool IsCorrectUsernameFormat(string username);
    }
}