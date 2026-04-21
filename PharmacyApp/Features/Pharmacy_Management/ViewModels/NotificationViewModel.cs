using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Pharmacy_Management.ViewModels
{
    public class NotificationViewModel
    {
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationButtonText { get; set; }

        public NotificationViewModel(string notificationTitle, string notificationBody, string notificationButtonText)
        {
            NotificationTitle = notificationTitle;
            NotificationBody = notificationBody;
            NotificationButtonText = notificationButtonText;
        }
    }
}
