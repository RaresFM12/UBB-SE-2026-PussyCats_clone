using PharmacyApp.Models;

namespace PharmacyApp.Features.Accounts.Logic
{
    public interface ICurrentUserService
    {
        User CurrentUser { get; }
    }
}