using System.Collections.Generic;
using PharmacyApp.Common.Services;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Pharmacy_Management.ViewModels
{
    public class NotificationsViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; }

        private IAdminService adminService;

        public NotificationsViewModel()
        {
            Notifications = new List<NotificationViewModel>();
            adminService = new AdminService();
        }

        public NotificationsViewModel(IAdminService adminService)
        {
            Notifications = new List<NotificationViewModel>();
            this.adminService = adminService;
        }

        public void PopulateNotifications()
        {
            User currentUser = ServiceWrapper.UserAccountService.CurrentUser;
            List<Notification> notificationData = adminService.GetNotificationsForUser(currentUser);

            foreach (Notification notification in notificationData)
            {
                Notifications.Add(new NotificationViewModel(notification.Title, notification.Message, notification.ActionButtonText));
            }
        }
    }
}
