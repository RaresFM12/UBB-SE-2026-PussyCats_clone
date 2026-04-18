using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class PeriodTrackerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CalendarsViewModel Calendars { get; set; }
        public ObservableCollection<NoteViewModel> Notes { get; }
        public ObservableCollection<ItemListViewModel> ItemsLists { get; set; }

        private const int MaxNotes = 4;

        public bool CanAddNote => Notes.Count < MaxNotes;

        private string _calendarsVisibility;
        [DefaultValue("Collapsed")]
        public string CalendarsVisibility
        {
            get { return _calendarsVisibility; }
            set
            {
                _calendarsVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _shopVisibility;
        [DefaultValue("Collapsed")]
        public string ShopVisibility
        {
            get { return _shopVisibility; }
            set
            {
                _shopVisibility = value;
                OnPropertyChanged();
            }
        }

        public PeriodTrackerViewModel()
        {
            Calendars = new CalendarsViewModel();
            Notes = new ObservableCollection<NoteViewModel>();
            ItemsLists = new ObservableCollection<ItemListViewModel>();
            CreateNotes();
            ShowCalendars();
        }

        private void CreateNotes()
        {
            foreach (KeyValuePair<int, Tuple<string, bool>> periodNote in PeriodTrackerUser.CurrentUser.PeriodNotes)
            {
                if (Notes.Count >= MaxNotes)
                    break;

                NoteViewModel periodNoteVM =
                    new NoteViewModel(periodNote.Key, periodNote.Value.Item1, periodNote.Value.Item2);

                Notes.Add(periodNoteVM);
            }

            OnPropertyChanged(nameof(CanAddNote));
        }

        private void CreateItems()
        {
            ItemsLists.Clear();
            ShopVisibility = "Visible";

            IItemsRepository itemsRepository = new SQLItemsRepository();
            List<Item> items = itemsRepository.GetAllItems();
            List<string> categories = new List<string>();
            categories.Add("wellness");
            items = items.Where(item => categories.Contains(item.Category)).ToList();

            int i = 0;
            while (i != items.Count)
            {
                ItemListViewModel itemListVM = new ItemListViewModel();

                int j = i + 4;
                while (i != items.Count && i != j)
                    itemListVM.Items.Add(new ItemViewModel(items[i++]));

                ItemsLists.Add(itemListVM);
            }

            int remaining = 0;
            while (i % 4 != 0)
            {
                ItemsLists[ItemsLists.Count - 1].Items.Add(new ItemViewModel(items[remaining++]));
                i++;
            }
        }

        private void ShowCalendars()
        {
            if (PeriodTrackerUser.HasPeriodTracker)
                CalculatePeriodTracker(PeriodTrackerUser.StartPeriodDate, PeriodTrackerUser.CycleDays,
                    PeriodTrackerUser.PeriodLasts, PeriodTrackerUser.PMSOption);

            CalendarsVisibility = PeriodTrackerUser.HasPeriodTracker ? "Visible" : "Collapsed";
        }

        internal void CalculatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts,
            int pmsOption)
        {
            PeriodTrackerUser.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, pmsOption);
            Calendars.CalculatePeriodTracker(startPeriodDate.Date);
            CreateItems();
        }

        internal void UpdatePeriodTracker(bool goRight)
        {
            Calendars.CurrentDate = Calendars.CurrentDate.AddMonths(goRight ? 1 : -1);
            Calendars.UpdatePeriodTracker(goRight);
        }

        internal void RemoveNote(NoteViewModel noteVM)
        {
            if (noteVM == null)
                return;

            Notes.Remove(noteVM);
            OnPropertyChanged(nameof(CanAddNote));
        }

        internal void AddNewNote()
        {
            if (Notes.Count >= MaxNotes)
                return;

            NoteViewModel newNote = new NoteViewModel(PeriodTrackerUser.MaxNoteId + 1, "", false);
            PeriodTrackerUser.CurrentUser.PeriodNotes[newNote.NoteId] =
                new Tuple<string, bool>("", false);
            PeriodTrackerUser.UpdateUser();
            Notes.Add(newNote);

            OnPropertyChanged(nameof(CanAddNote));
        }
    }
}