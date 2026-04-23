using System;
using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public interface IPeriodTrackerService
    {
        User GetCurrentUser();
        PeriodTrackerState GetTrackerState();
        Dictionary<int, Tuple<string, bool>> GetNotes();
        int GetMaxNoteId();
        void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption);
        void AddNote(string noteBody);
        void UpdateNote(int noteId, string noteBody, bool isDone);
        void DeleteNote(int noteId);
        void SaveCurrentUser();
    }
}