using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;
using System;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public static class PeriodTrackerUser
    {
        private static readonly IPeriodTrackerService service = new PeriodTrackerService();

        public static User CurrentUser => service.GetCurrentUser();

        public static DateTimeOffset StartPeriodDate => service.GetTrackerState().StartPeriodDate;

        public static int CycleDays => service.GetTrackerState().CycleDays;

        public static int PeriodLasts => service.GetTrackerState().PeriodLasts;

        public static int PMSOption => service.GetTrackerState().PmsOption;

        public static bool HasPeriodTracker => service.GetTrackerState().HasPeriodTracker;

        public static int MaxNoteId => service.GetMaxNoteId();

        public static void UpdateUser()
        {
            service.SaveCurrentUser();
        }

        public static void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int pmsOption)
        {
            service.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, pmsOption);
        }
    }
}