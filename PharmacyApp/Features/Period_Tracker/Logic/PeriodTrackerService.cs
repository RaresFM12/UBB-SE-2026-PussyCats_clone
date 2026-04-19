using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class PeriodTrackerService : IPeriodTrackerService
    {
        private const int DefaultCycleLengthInDays = 28;
        private const int DefaultPeriodLengthInDays = 5;
        private const int DefaultPmsOption = 0;
        private const int FirstNoteIdentifier = 1;
        private const int MidnightHour = 0;
        private const int MidnightMinute = 0;

        private readonly IUsersRepository userRepository;
        private readonly ICurrentUserService currentUserService;

        public PeriodTrackerService(
            IUsersRepository userRepository,
            ICurrentUserService currentUserService)
        {
            this.userRepository = userRepository;
            this.currentUserService = currentUserService;
        }

        public User GetCurrentUser()
        {
            return currentUserService.CurrentUser;
        }

        public PeriodTrackerState GetTrackerState()
        {
            User currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return CreateDefaultTrackerState();
            }

            DateTime trackerStartDate = GetTrackerStartDate(currentUser);

            return new PeriodTrackerState
            {
                StartPeriodDate = new DateTimeOffset(trackerStartDate),
                CycleDays = currentUser.CycleDays,
                PeriodLasts = currentUser.PeriodLasts,
                PmsOption = currentUser.PMSOption,
                HasPeriodTracker = userRepository.UserHasPeriodTracker(currentUser.Id)
            };
        }

        public Dictionary<int, Tuple<string, bool>> GetNotes()
        {
            User currentUser = GetCurrentUser();
            return currentUser?.PeriodNotes ?? new Dictionary<int, Tuple<string, bool>>();
        }

        public int GetMaxNoteId()
        {
            User currentUser = GetCurrentUser();

            if (currentUser == null || currentUser.PeriodNotes == null || currentUser.PeriodNotes.Count == 0)
            {
                return 0;
            }

            return currentUser.PeriodNotes.Keys.Max();
        }

        public void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int pmsOption)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return;
            }

            currentUser.SetPeriodTracker(
                DateOnly.FromDateTime(startPeriodDate.DateTime),
                Convert.ToInt32(cycleDays),
                Convert.ToInt32(periodLasts),
                pmsOption);

            SaveCurrentUser();
        }

        public void AddNote(string noteBody)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return;
            }

            int nextNoteIdentifier = GetNextNoteIdentifier();
            string safeNoteBody = noteBody ?? string.Empty;

            currentUser.AddPeriodNote(nextNoteIdentifier, safeNoteBody, false);
            SaveCurrentUser();
        }

        public void UpdateNote(int noteId, string noteBody, bool isDone)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.PeriodNotes == null)
            {
                return;
            }

            string safeNoteBody = noteBody ?? string.Empty;
            currentUser.PeriodNotes[noteId] = new Tuple<string, bool>(safeNoteBody, isDone);

            SaveCurrentUser();
        }

        public void DeleteNote(int noteId)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.PeriodNotes == null)
            {
                return;
            }

            if (!currentUser.PeriodNotes.ContainsKey(noteId))
            {
                return;
            }

            currentUser.PeriodNotes.Remove(noteId);
            SaveCurrentUser();
        }

        public void SaveCurrentUser()
        {
            User currentUser = GetCurrentUser();
            if (currentUser != null)
            {
                userRepository.UpdateUser(currentUser);
            }
        }

        private static PeriodTrackerState CreateDefaultTrackerState()
        {
            return new PeriodTrackerState
            {
                StartPeriodDate = new DateTimeOffset(DateTime.Today),
                CycleDays = DefaultCycleLengthInDays,
                PeriodLasts = DefaultPeriodLengthInDays,
                PmsOption = DefaultPmsOption,
                HasPeriodTracker = false
            };
        }

        private static DateTime GetTrackerStartDate(User currentUser)
        {
            bool userHasConfiguredStartDate = currentUser.StartPeriodDate.Year != default(DateOnly).Year;

            if (!userHasConfiguredStartDate)
            {
                return DateTime.Today;
            }

            return currentUser.StartPeriodDate.ToDateTime(new TimeOnly(MidnightHour, MidnightMinute));
        }

        private int GetNextNoteIdentifier()
        {
            int maximumExistingNoteIdentifier = GetMaxNoteId();

            if (maximumExistingNoteIdentifier == 0)
            {
                return FirstNoteIdentifier;
            }

            return maximumExistingNoteIdentifier + 1;
        }
    }
}