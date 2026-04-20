using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;
using System;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public static class PeriodTrackerUser
    {
        private static IPeriodTrackerService periodTrackerService;

        public static void Initialize(IPeriodTrackerService injectedPeriodTrackerService)
        {
            periodTrackerService = injectedPeriodTrackerService;
        }

        public static User CurrentUser => periodTrackerService.GetCurrentUser();

        public static DateTimeOffset StartPeriodDate => periodTrackerService.GetTrackerState().StartPeriodDate;

        public static int CycleDays => periodTrackerService.GetTrackerState().CycleDays;

        public static int PeriodLasts => periodTrackerService.GetTrackerState().PeriodLasts;

        public static int PMSOption => periodTrackerService.GetTrackerState().PmsOption;

        public static bool HasPeriodTracker => periodTrackerService.GetTrackerState().HasPeriodTracker;

        public static int MaxNoteId => periodTrackerService.GetMaxNoteId();

        public static void UpdateUser()
        {
            periodTrackerService.SaveCurrentUser();
        }

        public static void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int pmsOption)
        {
            periodTrackerService.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, pmsOption);
        }
    }
}