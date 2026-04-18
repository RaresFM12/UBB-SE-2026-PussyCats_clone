using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class CalendarsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private readonly Random rng;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CalendarsViewModel()
        {
            rng = new Random();
        }

        private DateTime _startPeriodDate;
        public DateTime StartPeriodDate
        {
            get { return _startPeriodDate; }
            set
            {
                _startPeriodDate = value;
                CurrentDate = DateTime.Today;
                OnPropertyChanged();
            }
        }

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime CurrentBeginningPeriodDate { get; set; }
        public DateTime CurrentEndPeriodDate { get; set; }
        public DateTime CurrentBeginningLowFertilityDate { get; set; }
        public DateTime CurrentEndLowFertilityDate { get; set; }
        public DateTime CurrentBeginningOvulationDate { get; set; }
        public DateTime CurrentEndOvulationDate { get; set; }
        public DateTime CurrentBeginningPMSDate { get; set; }
        public DateTime CurrentEndPMSDate { get; set; }

        public bool IsInMenstrualPhase =>
            DateTime.Today >= CurrentBeginningPeriodDate &&
            DateTime.Today <= CurrentEndPeriodDate;

        internal void CalculatePeriodTracker(DateTime startPeriodDate)
        {
            StartPeriodDate = startPeriodDate.Date;
            CurrentBeginningPeriodDate = StartPeriodDate.AddDays(-PeriodTrackerUser.CycleDays);
            CurrentDate = DateTime.Today;

            UpdatePeriodTracker(true);
        }

        internal void UpdatePeriodTracker(bool goRight)
        {
            LiterallyTodayString = DateTime.Today.ToString("d");

            CurrentBeginningPeriodDate =
                CurrentBeginningPeriodDate.AddDays(goRight ? (int)PeriodTrackerUser.CycleDays : -(int)PeriodTrackerUser.CycleDays);

            CurrentMonth = CurrentBeginningPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture);

            CurrentEndPeriodDate = new DateTime(
                CurrentBeginningPeriodDate.Year,
                CurrentBeginningPeriodDate.Month,
                CurrentBeginningPeriodDate.Day).AddDays(PeriodTrackerUser.PeriodLasts);

            PeriodInterval = "Period Days: " +
                $"{CurrentBeginningPeriodDate.Day} {CurrentBeginningPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentBeginningPeriodDate.Year} - " +
                $"{CurrentEndPeriodDate.Day} {CurrentEndPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentEndPeriodDate.Year}";

            NextPeriodDateString = CurrentBeginningPeriodDate.AddDays(PeriodTrackerUser.CycleDays).ToString("d");
            NextPeriodDistanceString = "In " +
                $"{double.Ceiling((CurrentBeginningPeriodDate.AddDays(PeriodTrackerUser.CycleDays) - DateTime.Today).TotalDays)} days";

            if (PeriodTrackerUser.PeriodLasts < 9)
            {
                CurrentBeginningLowFertilityDate = new DateTime(
                    CurrentEndPeriodDate.Year,
                    CurrentEndPeriodDate.Month,
                    CurrentEndPeriodDate.Day).AddDays(1);

                CurrentEndLowFertilityDate = new DateTime(
                    CurrentBeginningPeriodDate.Year,
                    CurrentBeginningPeriodDate.Month,
                    CurrentBeginningPeriodDate.Day).AddDays(8);

                LowFertilityInterval = "Low Fertility Days: " +
                    $"{CurrentBeginningLowFertilityDate.Day} {CurrentBeginningLowFertilityDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentBeginningLowFertilityDate.Year} - " +
                    $"{CurrentEndLowFertilityDate.Day} {CurrentEndLowFertilityDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentEndLowFertilityDate.Year}";
            }
            else
            {
                LowFertilityInterval = "Low Fertility Days: No such days";
            }

            CurrentBeginningOvulationDate = new DateTime(
                CurrentBeginningPeriodDate.Year,
                CurrentBeginningPeriodDate.Month,
                CurrentBeginningPeriodDate.Day).AddDays(11);

            CurrentEndOvulationDate = new DateTime(
                CurrentEndPeriodDate.Year,
                CurrentEndPeriodDate.Month,
                CurrentEndPeriodDate.Day).AddDays(15);

            OvulationInterval = "Ovulation Days: " +
                $"{CurrentBeginningOvulationDate.Day} {CurrentBeginningOvulationDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentBeginningOvulationDate.Year} - " +
                $"{CurrentEndOvulationDate.Day} {CurrentEndOvulationDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentEndOvulationDate.Year}";

            CurrentOvulationDateString = CurrentBeginningOvulationDate.ToString("d");

            if (DateTime.Today >= CurrentBeginningOvulationDate && DateTime.Today <= CurrentEndOvulationDate)
                PastOvulationString = "Now";
            else if (DateTime.Today > CurrentEndOvulationDate)
                PastOvulationString = "Passed";
            else
                PastOvulationString = "Upcoming";

            if (PeriodTrackerUser.PMSOption != 0)
            {
                CurrentBeginningPMSDate = new DateTime(
                    CurrentBeginningPeriodDate.Year,
                    CurrentBeginningPeriodDate.Month,
                    CurrentBeginningPeriodDate.Day).AddDays(PeriodTrackerUser.CycleDays - 1);

                if (PeriodTrackerUser.PMSOption == 1)
                    CurrentBeginningPMSDate = CurrentBeginningPMSDate.AddDays(-rng.Next(1, 4));
                else if (PeriodTrackerUser.PMSOption == 2)
                    CurrentBeginningPMSDate = CurrentBeginningPMSDate.AddDays(-rng.Next(4, 8));
                else
                    CurrentBeginningPMSDate = CurrentBeginningPMSDate.AddDays(-rng.Next(7, 15));

                CurrentEndPMSDate = new DateTime(
                    CurrentBeginningPeriodDate.Year,
                    CurrentBeginningPeriodDate.Month,
                    CurrentBeginningPeriodDate.Day).AddDays(PeriodTrackerUser.CycleDays);

                PmsInterval = "PMS Days: " +
                    $"{CurrentBeginningPMSDate.Day} {CurrentBeginningPMSDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentBeginningPMSDate.Year} - " +
                    $"{CurrentEndPMSDate.Day} {CurrentEndPMSDate.ToString("MMMM", CultureInfo.InvariantCulture)} {CurrentEndPMSDate.Year}";
            }
            else
            {
                PmsInterval = "PMS Days: No such days";
            }

            ConstructCurrentPhaseString();
        }

        private void ConstructCurrentPhaseString()
        {
            if (DateTime.Today >= CurrentBeginningPeriodDate && DateTime.Today <= CurrentEndPeriodDate)
                CurrentPhaseString = "Menstrual Phase";
            else if (DateTime.Today > CurrentEndPeriodDate && DateTime.Today < CurrentBeginningOvulationDate)
                CurrentPhaseString = "Follicular Phase";
            else if (DateTime.Today >= CurrentBeginningOvulationDate && DateTime.Today <= CurrentEndOvulationDate)
                CurrentPhaseString = "Ovulation Phase";
            else if (DateTime.Today > CurrentEndOvulationDate && DateTime.Today < CurrentBeginningPeriodDate.AddDays(PeriodTrackerUser.CycleDays))
                CurrentPhaseString = "Luteal Phase";
            else
                CurrentPhaseString = "Not calculated for this month";

            OnPropertyChanged(nameof(IsInMenstrualPhase));
        }

        private string _currentMonth;
        public string CurrentMonth
        {
            get { return _currentMonth; }
            set { _currentMonth = value; OnPropertyChanged(); }
        }

        private string _periodInterval;
        public string PeriodInterval
        {
            get { return _periodInterval; }
            set { _periodInterval = value; OnPropertyChanged(); }
        }

        private string _lowFertilityInterval;
        public string LowFertilityInterval
        {
            get { return _lowFertilityInterval; }
            set { _lowFertilityInterval = value; OnPropertyChanged(); }
        }

        private string _ovulationInterval;
        public string OvulationInterval
        {
            get { return _ovulationInterval; }
            set { _ovulationInterval = value; OnPropertyChanged(); }
        }

        private string _pmsInterval;
        public string PmsInterval
        {
            get { return _pmsInterval; }
            set { _pmsInterval = value; OnPropertyChanged(); }
        }

        private string _pastOvulationString;
        public string PastOvulationString
        {
            get { return _pastOvulationString; }
            set { _pastOvulationString = value; OnPropertyChanged(); }
        }

        private string _nextPeriodDateString;
        public string NextPeriodDateString
        {
            get { return _nextPeriodDateString; }
            set { _nextPeriodDateString = value; OnPropertyChanged(); }
        }

        private string _currentPhaseString;
        public string CurrentPhaseString
        {
            get { return _currentPhaseString; }
            set { _currentPhaseString = value; OnPropertyChanged(); }
        }

        private string _literallyTodayString;
        public string LiterallyTodayString
        {
            get { return _literallyTodayString; }
            set { _literallyTodayString = value; OnPropertyChanged(); }
        }

        private string _nextPeriodDistanceString;
        public string NextPeriodDistanceString
        {
            get { return _nextPeriodDistanceString; }
            set { _nextPeriodDistanceString = value; OnPropertyChanged(); }
        }

        private string _currentOvulationDateString;
        public string CurrentOvulationDateString
        {
            get { return _currentOvulationDateString; }
            set { _currentOvulationDateString = value; OnPropertyChanged(); }
        }
    }
}