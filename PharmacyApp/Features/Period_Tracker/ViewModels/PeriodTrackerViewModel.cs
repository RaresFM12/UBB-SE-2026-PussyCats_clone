using Microsoft.UI.Xaml;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;
using Syncfusion.UI.Xaml.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class PeriodTrackerViewModel : INotifyPropertyChanged
    {
        private readonly IPeriodTrackerService periodTrackerService;
        private readonly IWellnessItemsService wellnessItemsService;
        private readonly IBasketService basketService;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private const int MaxNotes = 4;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CalendarsViewModel Calendars { get; }

        public ObservableCollection<NoteViewModel> Notes { get; }

        public ObservableCollection<ItemListViewModel> ItemsLists { get; }

        public ICommand CalculateCommand { get; }
        public ICommand NextCycleCommand { get; }
        public ICommand PreviousCycleCommand { get; }
        public ICommand AddNoteCommand { get; }

        public bool CanAddNote => Notes.Count < MaxNotes;
        public Visibility AddNoteVisibility => CanAddNote ? Visibility.Visible : Visibility.Collapsed;

        private Visibility calendarsVisibility = Visibility.Collapsed;
        public Visibility CalendarsVisibility
        {
            get => calendarsVisibility;
            set
            {
                calendarsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility shopVisibility = Visibility.Collapsed;
        public Visibility ShopVisibility
        {
            get => shopVisibility;
            set
            {
                shopVisibility = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset startPeriodDate;
        public DateTimeOffset StartPeriodDate
        {
            get => startPeriodDate;
            set
            {
                startPeriodDate = value;
                OnPropertyChanged();
            }
        }

        private double cycleDaysInput;
        public double CycleDaysInput
        {
            get => cycleDaysInput;
            set
            {
                cycleDaysInput = value;
                OnPropertyChanged();
            }
        }

        private double periodLastsInput;
        public double PeriodLastsInput
        {
            get => periodLastsInput;
            set
            {
                periodLastsInput = value;
                OnPropertyChanged();
            }
        }

        private int pmsOptionInput;
        public int PMSOptionInput
        {
            get => pmsOptionInput;
            set
            {
                pmsOptionInput = value;
                OnPropertyChanged();
            }
        }

        public PeriodTrackerViewModel()
            : this(new PeriodTrackerService(), new WellnessItemsService(), new BasketService())
        {
        }

        public PeriodTrackerViewModel(
            IPeriodTrackerService periodTrackerService,
            IWellnessItemsService wellnessItemsService,
            IBasketService basketService)
        {
            this.periodTrackerService = periodTrackerService;
            this.wellnessItemsService = wellnessItemsService;
            this.basketService = basketService;

            Calendars = new CalendarsViewModel();
            Notes = new ObservableCollection<NoteViewModel>();
            ItemsLists = new ObservableCollection<ItemListViewModel>();

            CalculateCommand = new DelegateCommand(_ => CalculatePeriodTracker());
            NextCycleCommand = new DelegateCommand(_ => UpdatePeriodTracker(true));
            PreviousCycleCommand = new DelegateCommand(_ => UpdatePeriodTracker(false));
            AddNoteCommand = new DelegateCommand(_ => AddNewNote());

            LoadInitialState();
        }

        private void LoadInitialState()
        {
            PeriodTrackerState state = periodTrackerService.GetTrackerState();

            StartPeriodDate = state.StartPeriodDate;
            CycleDaysInput = state.CycleDays;
            PeriodLastsInput = state.PeriodLasts;
            PMSOptionInput = state.PmsOption;

            LoadNotes();

            if (state.HasPeriodTracker)
            {
                Calendars.CalculatePeriodTracker(
                    StartPeriodDate.Date,
                    (int)CycleDaysInput,
                    (int)PeriodLastsInput,
                    PMSOptionInput);

                CalendarsVisibility = Visibility.Visible;
                BuildItems();
            }
            else
            {
                CalendarsVisibility = Visibility.Collapsed;
                ShopVisibility = Visibility.Collapsed;
            }
        }

        private void LoadNotes()
        {
            Notes.Clear();

            foreach (KeyValuePair<int, Tuple<string, bool>> note in periodTrackerService.GetNotes()
                         .OrderBy(entry => entry.Key)
                         .Take(MaxNotes))
            {
                Notes.Add(new NoteViewModel(
                    note.Key,
                    note.Value.Item1,
                    note.Value.Item2,
                    DeleteNote,
                    UpdateNote));
            }

            OnPropertyChanged(nameof(CanAddNote));
            OnPropertyChanged(nameof(AddNoteVisibility));
        }

        private void CalculatePeriodTracker()
        {
            periodTrackerService.UpdatePeriodTracker(StartPeriodDate, CycleDaysInput, PeriodLastsInput, PMSOptionInput);

            Calendars.CalculatePeriodTracker(
                StartPeriodDate.Date,
                (int)CycleDaysInput,
                (int)PeriodLastsInput,
                PMSOptionInput);

            CalendarsVisibility = Visibility.Visible;
            BuildItems();
        }

        private void UpdatePeriodTracker(bool goRight)
        {
            if (CalendarsVisibility != Visibility.Visible)
            {
                return;
            }

            Calendars.UpdatePeriodTracker(goRight);
            BuildItems();
        }

        private void BuildItems()
        {
            ItemsLists.Clear();

            List<Item> items = wellnessItemsService.GetWellnessItems();

            if (items.Count == 0)
            {
                ShopVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(ItemsLists));
                return;
            }

            ShopVisibility = Visibility.Visible;

            float extraDiscount = Calendars.IsInMenstrualPhase ? 20.0f : 0.0f;

            for (int i = 0; i < items.Count; i += 4)
            {
                ItemListViewModel row = new ItemListViewModel();

                foreach (Item item in items.Skip(i).Take(4))
                {
                    row.Items.Add(new ItemViewModel(item, extraDiscount, basketService));
                }

                ItemsLists.Add(row);
            }

            OnPropertyChanged(nameof(ItemsLists));
        }

        private void AddNewNote()
        {
            if (Notes.Count >= MaxNotes)
            {
                return;
            }

            periodTrackerService.AddNote(string.Empty);
            LoadNotes();
        }

        private void UpdateNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            periodTrackerService.UpdateNote(note.NoteId, note.NoteBody, note.NoteIsDone);
        }

        private void DeleteNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            periodTrackerService.DeleteNote(note.NoteId);
            LoadNotes();
        }
    }
}