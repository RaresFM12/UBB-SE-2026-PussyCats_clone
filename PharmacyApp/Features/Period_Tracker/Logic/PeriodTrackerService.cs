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
        private readonly IUsersRepository usersRepository;

        public PeriodTrackerService()
            : this(new SQLUsersRepository())
        {
        }

        public PeriodTrackerService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public User GetCurrentUser()
        {
            return ServiceWrapper.UserAccountService.CurrentUser;
        }

        public PeriodTrackerState GetTrackerState()
        {
            User currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return new PeriodTrackerState
                {
                    StartPeriodDate = new DateTimeOffset(DateTime.Today),
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PmsOption = 0,
                    HasPeriodTracker = false
                };
            }

            DateTime trackerDate = currentUser.StartPeriodDate.Year == new DateOnly().Year
                ? DateTime.Today
                : currentUser.StartPeriodDate.ToDateTime(new TimeOnly(0));

            return new PeriodTrackerState
            {
                StartPeriodDate = new DateTimeOffset(trackerDate),
                CycleDays = currentUser.CycleDays,
                PeriodLasts = currentUser.PeriodLasts,
                PmsOption = currentUser.PMSOption,
                HasPeriodTracker = usersRepository.UserHasPeriodTracker(currentUser.Id)
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

            if (currentUser == null || currentUser.PeriodNotes.Count == 0)
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
                (int)cycleDays,
                (int)periodLasts,
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

            int nextId = GetMaxNoteId() + 1;
            currentUser.AddPeriodNote(nextId, noteBody ?? string.Empty, false);
            SaveCurrentUser();
        }

        public void UpdateNote(int noteId, string noteBody, bool isDone)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return;
            }

            currentUser.PeriodNotes[noteId] = new Tuple<string, bool>(noteBody ?? string.Empty, isDone);
            SaveCurrentUser();
        }

        public void DeleteNote(int noteId)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return;
            }

            if (currentUser.PeriodNotes.ContainsKey(noteId))
            {
                currentUser.PeriodNotes.Remove(noteId);
                SaveCurrentUser();
            }
        }

        public void SaveCurrentUser()
        {
            User currentUser = GetCurrentUser();
            if (currentUser != null)
            {
                usersRepository.UpdateUser(currentUser);
            }
        }
    }
}