using System;

namespace PharmacyApp.Models
{
    public class Notification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionButtonText { get; set; }

        public Notification(string title, string message, string actionButtonText = "")
        {
            Title = title;
            Message = message;
            ActionButtonText = actionButtonText;
        }
    }
}
