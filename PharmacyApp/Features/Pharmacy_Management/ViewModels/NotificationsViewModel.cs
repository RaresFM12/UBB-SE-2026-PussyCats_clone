using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PharmacyApp.Common.Services;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Pharmacy_Management.ViewModels
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NotificationViewModel> notifications;

        public ObservableCollection<NotificationViewModel> Notifications
        {
            get => notifications;
            set
            {
                notifications = value;
                OnPropertyChanged();
            }
        }

        private readonly IAdminService adminService;

        public NotificationsViewModel(IAdminService adminService)
        {
            Notifications = new ObservableCollection<NotificationViewModel>();
            this.adminService = adminService;
        }

        public void PopulateNotifications()
        {
            User currentUser = ServiceWrapper.UserAccountService.CurrentUser;
            List<Notification> notificationData = adminService.GetNotificationsForUser(currentUser);

            Notifications.Clear();

            foreach (Notification notification in notificationData)
            {
                Notifications.Add(new NotificationViewModel(notification.Title, notification.Message, notification.ActionButtonText));
            }
        }

        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}