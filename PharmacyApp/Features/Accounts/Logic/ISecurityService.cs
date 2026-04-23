using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Accounts.Logic
{
    public interface ISecurityService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string stored);
    }
}
