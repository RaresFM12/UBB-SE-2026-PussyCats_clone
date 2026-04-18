using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
        public string AddNoteVisibility => CanAddNote ? "Visible" : "Collapsed";

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
            OnPropertyChanged(nameof(AddNoteVisibility));
        }

        private void CreateItems()
        {
            ItemsLists.Clear();

            IItemsRepository itemsRepository = new SQLItemsRepository();
            List<Item> items = itemsRepository.GetAllItems()
                .Where(item => item.Category != null &&
                               item.Category.Equals("wellness", StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Id)
                .ToList();

            if (items.Count == 0)
            {
                ShopVisibility = "Collapsed";
                OnPropertyChanged(nameof(ItemsLists));
                return;
            }

            ShopVisibility = "Visible";

            float extraDiscount = Calendars.IsInMenstrualPhase ? 20.0f : 0.0f;

            for (int i = 0; i < items.Count; i += 4)
            {
                ItemListViewModel itemListVM = new ItemListViewModel();

                int localIndex = 0;
                foreach (Item item in items.Skip(i).Take(4))
                {
                    ItemViewModel itemVm = new ItemViewModel(item, extraDiscount)
                    {
                        AddToBasketCommand = itemListVM.AddItemToBasket,
                        ItemIndex = localIndex
                    };

                    itemListVM.Items.Add(itemVm);
                    localIndex++;
                }

                ItemsLists.Add(itemListVM);
            }

            OnPropertyChanged(nameof(ItemsLists));
        }

        private void ShowCalendars()
        {
            if (PeriodTrackerUser.HasPeriodTracker)
                CalculatePeriodTracker(
                    PeriodTrackerUser.StartPeriodDate,
                    PeriodTrackerUser.CycleDays,
                    PeriodTrackerUser.PeriodLasts,
                    PeriodTrackerUser.PMSOption);

            CalendarsVisibility = PeriodTrackerUser.HasPeriodTracker ? "Visible" : "Collapsed";
        }

        internal void CalculatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int pmsOption)
        {
            PeriodTrackerUser.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, pmsOption);
            Calendars.CalculatePeriodTracker(startPeriodDate.Date);
            CreateItems();
        }

        internal void UpdatePeriodTracker(bool goRight)
        {
            Calendars.CurrentDate = Calendars.CurrentDate.AddMonths(goRight ? 1 : -1);
            Calendars.UpdatePeriodTracker(goRight);
            CreateItems();
        }

        internal void RemoveNote(NoteViewModel noteVM)
        {
            if (noteVM == null)
                return;

            Notes.Remove(noteVM);
            OnPropertyChanged(nameof(CanAddNote));
            OnPropertyChanged(nameof(AddNoteVisibility));
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
            OnPropertyChanged(nameof(AddNoteVisibility));
        }
    }
}