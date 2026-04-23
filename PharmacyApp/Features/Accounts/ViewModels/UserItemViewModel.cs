using PharmacyApp.Models;

namespace PharmacyApp.Features.Accounts.ViewModels
{
    public class UserItemViewModel
    {
        private User user;

        public UserItemViewModel(User user)
        {
            this.user = user;
        }

        public User User => user;

        public string Email => user.Email;
        public string Username => string.IsNullOrEmpty(user.Username) ? "(no username)" : user.Username;
        public string PhoneNumber => string.IsNullOrEmpty(user.PhoneNumber) ? "(no phone)" : user.PhoneNumber;

        public bool IsAdmin => user.IsAdmin;
        public bool IsDisabled => user.IsDisabled;

        public double Opacity => IsDisabled ? 0.7 : 1.0;

        public string Background
        {
            get
            {
                if (IsDisabled)
                {
                    return "#E8F5E9";
                }

                if (IsAdmin)
                {
                    return "#FFF8E1";
                }

                return "#F4F8F6";
            }
        }

        public bool ShowPromote => !IsAdmin && !IsDisabled;

        public bool ShowDisable => !IsAdmin && !IsDisabled;

        public bool ShowDisabledLabel => IsDisabled;
    }
}
