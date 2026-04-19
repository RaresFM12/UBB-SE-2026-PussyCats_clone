using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class CalendarsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private readonly Random rng = new Random();

        private DateTime startPeriodDate;
        private int cycleDays;
        private int periodLasts;
        private int pmsOption;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DateTime currentDate;
        public DateTime CurrentDate
        {
            get => currentDate;
            set
            {
                currentDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime CurrentBeginningPeriodDate { get; private set; }
        public DateTime CurrentEndPeriodDate { get; private set; }
        public DateTime CurrentBeginningLowFertilityDate { get; private set; }
        public DateTime CurrentEndLowFertilityDate { get; private set; }
        public DateTime CurrentBeginningOvulationDate { get; private set; }
        public DateTime CurrentEndOvulationDate { get; private set; }
        public DateTime CurrentBeginningPmsDate { get; private set; }
        public DateTime CurrentEndPmsDate { get; private set; }

        public bool IsInMenstrualPhase =>
            DateTime.Today >= CurrentBeginningPeriodDate &&
            DateTime.Today <= CurrentEndPeriodDate;

        public void CalculatePeriodTracker(DateTime startDate, int cycleDays, int periodLasts, int pmsOption)
        {
            startPeriodDate = startDate.Date;
            this.cycleDays = cycleDays;
            this.periodLasts = periodLasts;
            this.pmsOption = pmsOption;

            CurrentDate = DateTime.Today;
            CurrentBeginningPeriodDate = startPeriodDate;

            while (CurrentBeginningPeriodDate.AddDays(this.cycleDays) <= DateTime.Today)
            {
                CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(this.cycleDays);
            }

            while (CurrentBeginningPeriodDate > DateTime.Today)
            {
                CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(-this.cycleDays);
            }

            RecalculateDisplayStrings();
        }

        public void UpdatePeriodTracker(bool goRight)
        {
            if (cycleDays <= 0)
            {
                return;
            }

            CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(goRight ? cycleDays : -cycleDays);
            RecalculateDisplayStrings();
        }

        private void RecalculateDisplayStrings()
        {
            LiterallyTodayString = DateTime.Today.ToString("d");
            CurrentMonth = CurrentBeginningPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture);

            CurrentEndPeriodDate = CurrentBeginningPeriodDate.AddDays(periodLasts);

            PeriodInterval =
                $"Period Days: {CurrentBeginningPeriodDate.Day} {CurrentBeginningPeriodDate:MMMM} {CurrentBeginningPeriodDate.Year} - " +
                $"{CurrentEndPeriodDate.Day} {CurrentEndPeriodDate:MMMM} {CurrentEndPeriodDate.Year}";

            DateTime nextPeriodDate = CurrentBeginningPeriodDate.AddDays(cycleDays);
            NextPeriodDateString = nextPeriodDate.ToString("d");
            NextPeriodDistanceString = $"In {Math.Max(0, Math.Ceiling((nextPeriodDate - DateTime.Today).TotalDays))} days";

            if (periodLasts < 9)
            {
                CurrentBeginningLowFertilityDate = CurrentEndPeriodDate.AddDays(1);
                CurrentEndLowFertilityDate = CurrentBeginningPeriodDate.AddDays(8);

                LowFertilityInterval =
                    $"Low Fertility Days: {CurrentBeginningLowFertilityDate.Day} {CurrentBeginningLowFertilityDate:MMMM} {CurrentBeginningLowFertilityDate.Year} - " +
                    $"{CurrentEndLowFertilityDate.Day} {CurrentEndLowFertilityDate:MMMM} {CurrentEndLowFertilityDate.Year}";
            }
            else
            {
                LowFertilityInterval = "Low Fertility Days: No such days";
            }

            CurrentBeginningOvulationDate = CurrentBeginningPeriodDate.AddDays(11);
            CurrentEndOvulationDate = CurrentBeginningPeriodDate.AddDays(15);

            OvulationInterval =
                $"Ovulation Days: {CurrentBeginningOvulationDate.Day} {CurrentBeginningOvulationDate:MMMM} {CurrentBeginningOvulationDate.Year} - " +
                $"{CurrentEndOvulationDate.Day} {CurrentEndOvulationDate:MMMM} {CurrentEndOvulationDate.Year}";

            CurrentOvulationDateString = CurrentBeginningOvulationDate.ToString("d");

            if (DateTime.Today >= CurrentBeginningOvulationDate && DateTime.Today <= CurrentEndOvulationDate)
            {
                PastOvulationString = "Now";
            }
            else if (DateTime.Today > CurrentEndOvulationDate)
            {
                PastOvulationString = "Passed";
            }
            else
            {
                PastOvulationString = "Upcoming";
            }

            if (pmsOption != 0)
            {
                CurrentBeginningPmsDate = CurrentBeginningPeriodDate.AddDays(cycleDays - 1);

                if (pmsOption == 1)
                {
                    CurrentBeginningPmsDate = CurrentBeginningPmsDate.AddDays(-rng.Next(1, 4));
                }
                else if (pmsOption == 2)
                {
                    CurrentBeginningPmsDate = CurrentBeginningPmsDate.AddDays(-rng.Next(4, 8));
                }
                else
                {
                    CurrentBeginningPmsDate = CurrentBeginningPmsDate.AddDays(-rng.Next(7, 15));
                }

                CurrentEndPmsDate = CurrentBeginningPeriodDate.AddDays(cycleDays);

                PmsInterval =
                    $"PMS Days: {CurrentBeginningPmsDate.Day} {CurrentBeginningPmsDate:MMMM} {CurrentBeginningPmsDate.Year} - " +
                    $"{CurrentEndPmsDate.Day} {CurrentEndPmsDate:MMMM} {CurrentEndPmsDate.Year}";
            }
            else
            {
                PmsInterval = "PMS Days: No such days";
            }

            ConstructCurrentPhaseString();
            OnPropertyChanged(nameof(IsInMenstrualPhase));
        }

        private void ConstructCurrentPhaseString()
        {
            DateTime today = DateTime.Today;

            if (today >= CurrentBeginningPeriodDate && today <= CurrentEndPeriodDate)
            {
                CurrentPhaseString = "Menstrual Phase";
            }
            else if (today > CurrentEndPeriodDate && today < CurrentBeginningOvulationDate)
            {
                CurrentPhaseString = "Follicular Phase";
            }
            else if (today >= CurrentBeginningOvulationDate && today <= CurrentEndOvulationDate)
            {
                CurrentPhaseString = "Ovulation Phase";
            }
            else if (today > CurrentEndOvulationDate && today < CurrentBeginningPeriodDate.AddDays(cycleDays))
            {
                CurrentPhaseString = "Luteal Phase";
            }
            else
            {
                CurrentPhaseString = "Not calculated for this month";
            }
        }

        private string currentMonth;
        public string CurrentMonth
        {
            get => currentMonth;
            set
            {
                currentMonth = value;
                OnPropertyChanged();
            }
        }

        private string periodInterval;
        public string PeriodInterval
        {
            get => periodInterval;
            set
            {
                periodInterval = value;
                OnPropertyChanged();
            }
        }

        private string lowFertilityInterval;
        public string LowFertilityInterval
        {
            get => lowFertilityInterval;
            set
            {
                lowFertilityInterval = value;
                OnPropertyChanged();
            }
        }

        private string ovulationInterval;
        public string OvulationInterval
        {
            get => ovulationInterval;
            set
            {
                ovulationInterval = value;
                OnPropertyChanged();
            }
        }

        private string pmsInterval;
        public string PmsInterval
        {
            get => pmsInterval;
            set
            {
                pmsInterval = value;
                OnPropertyChanged();
            }
        }

        private string pastOvulationString;
        public string PastOvulationString
        {
            get => pastOvulationString;
            set
            {
                pastOvulationString = value;
                OnPropertyChanged();
            }
        }

        private string nextPeriodDateString;
        public string NextPeriodDateString
        {
            get => nextPeriodDateString;
            set
            {
                nextPeriodDateString = value;
                OnPropertyChanged();
            }
        }

        private string currentPhaseString;
        public string CurrentPhaseString
        {
            get => currentPhaseString;
            set
            {
                currentPhaseString = value;
                OnPropertyChanged();
            }
        }

        private string literallyTodayString;
        public string LiterallyTodayString
        {
            get => literallyTodayString;
            set
            {
                literallyTodayString = value;
                OnPropertyChanged();
            }
        }

        private string nextPeriodDistanceString;
        public string NextPeriodDistanceString
        {
            get => nextPeriodDistanceString;
            set
            {
                nextPeriodDistanceString = value;
                OnPropertyChanged();
            }
        }

        private string currentOvulationDateString;
        public string CurrentOvulationDateString
        {
            get => currentOvulationDateString;
            set
            {
                currentOvulationDateString = value;
                OnPropertyChanged();
            }
        }
    }
}