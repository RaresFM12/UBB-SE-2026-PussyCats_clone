using PharmacyApp.Features.Period_Tracker.ViewModels;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.ViewModels
{
    [TestFixture]
    public class CalendarsViewModelTests
    {
        [Test]
        public void CalculatePeriodTracker_WhenStartDateIsInPast_AlignsCurrentBeginningPeriodDateToCurrentCycle()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime originalStartDate = DateTime.Today.AddDays(-60);

            viewModel.CalculatePeriodTracker(originalStartDate, 28, 5, 0);

            Assert.That(
                CurrentCycleContainsToday(viewModel.CurrentBeginningPeriodDate, 28),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenPeriodLengthIsLessThanNine_ComputesLowFertilityInterval()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();

            viewModel.CalculatePeriodTracker(DateTime.Today, 28, 5, 0);

            Assert.That(
                IsComputedLowFertilityInterval(viewModel.LowFertilityInterval),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenPeriodLengthIsAtLeastNine_SetsNoLowFertilityDaysText()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();

            viewModel.CalculatePeriodTracker(DateTime.Today, 28, 9, 0);

            Assert.That(viewModel.LowFertilityInterval, Is.EqualTo("Low Fertility Days: No such days"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenPremenstrualSyndromeOptionIsZero_SetsNoPmsDaysText()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();

            viewModel.CalculatePeriodTracker(DateTime.Today, 28, 5, 0);

            Assert.That(viewModel.PmsInterval, Is.EqualTo("PMS Days: No such days"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenPremenstrualSyndromeOptionIsMild_ComputesPmsDatesWithinExpectedRangeAndText()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime cycleStart = DateTime.Today;

            viewModel.CalculatePeriodTracker(cycleStart, 28, 5, 1);

            DateTime nextCycleStart = viewModel.CurrentBeginningPeriodDate.AddDays(28);
            double daysBeforeNextPeriod = (nextCycleStart - viewModel.CurrentBeginningPmsDate).TotalDays;

            Assert.That(
                MatchesPmsComputation(daysBeforeNextPeriod, viewModel.PmsInterval, 2, 4),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenTodayFallsInsideOvulationWindow_SetsPastOvulationStringToNow()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-12);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.PastOvulationString, Is.EqualTo("Now"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenOvulationAlreadyPassed_SetsPastOvulationStringToPassed()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-20);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.PastOvulationString, Is.EqualTo("Passed"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenOvulationIsStillUpcoming_SetsPastOvulationStringToUpcoming()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-3);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.PastOvulationString, Is.EqualTo("Upcoming"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenTodayIsWithinPeriod_SetsCurrentPhaseToMenstrualPhase()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-1);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.CurrentPhaseString, Is.EqualTo("Menstrual Phase"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenTodayIsBetweenPeriodAndOvulation_SetsCurrentPhaseToFollicularPhase()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-7);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.CurrentPhaseString, Is.EqualTo("Follicular Phase"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenTodayIsWithinOvulation_SetsCurrentPhaseToOvulationPhase()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-12);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.CurrentPhaseString, Is.EqualTo("Ovulation Phase"));
        }

        [Test]
        public void CalculatePeriodTracker_WhenTodayIsAfterOvulationBeforeNextPeriod_SetsCurrentPhaseToLutealPhase()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime startDate = DateTime.Today.AddDays(-18);

            viewModel.CalculatePeriodTracker(startDate, 28, 5, 0);

            Assert.That(viewModel.CurrentPhaseString, Is.EqualTo("Luteal Phase"));
        }

        [Test]
        public void UpdatePeriodTracker_WhenNavigatingToNextCycle_MovesBeginningDateForwardByCycleLength()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            viewModel.CalculatePeriodTracker(DateTime.Today.AddDays(-10), 28, 5, 0);

            DateTime initialBeginningDate = viewModel.CurrentBeginningPeriodDate;

            viewModel.UpdatePeriodTracker(true);

            Assert.That(viewModel.CurrentBeginningPeriodDate, Is.EqualTo(initialBeginningDate.AddDays(28)));
        }

        [Test]
        public void UpdatePeriodTracker_WhenNavigatingToPreviousCycle_MovesBeginningDateBackwardByCycleLength()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            viewModel.CalculatePeriodTracker(DateTime.Today.AddDays(-10), 28, 5, 0);

            DateTime initialBeginningDate = viewModel.CurrentBeginningPeriodDate;

            viewModel.UpdatePeriodTracker(false);

            Assert.That(viewModel.CurrentBeginningPeriodDate, Is.EqualTo(initialBeginningDate.AddDays(-28)));
        }

        [Test]
        public void UpdatePeriodTracker_WhenCycleWasNotCalculated_DoesNothing()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime initialDate = viewModel.CurrentBeginningPeriodDate;

            viewModel.UpdatePeriodTracker(true);

            Assert.That(viewModel.CurrentBeginningPeriodDate, Is.EqualTo(initialDate));
        }

        [Test]
        public void CalculatePeriodTracker_WhenStartDateIsInFuture_AlignsCurrentBeginningPeriodDateBackwardToCurrentCycle()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime futureStartDate = DateTime.Today.AddDays(60);

            viewModel.CalculatePeriodTracker(futureStartDate, 28, 5, 0);

            Assert.That(
                CurrentCycleContainsToday(viewModel.CurrentBeginningPeriodDate, 28),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenPremenstrualSyndromeOptionIsModerate_ComputesPmsDatesWithinExpectedRangeAndText()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime cycleStart = DateTime.Today;

            viewModel.CalculatePeriodTracker(cycleStart, 28, 5, 2);

            DateTime nextCycleStart = viewModel.CurrentBeginningPeriodDate.AddDays(28);
            double daysBeforeNextPeriod = (nextCycleStart - viewModel.CurrentBeginningPmsDate).TotalDays;

            Assert.That(
                MatchesPmsComputation(daysBeforeNextPeriod, viewModel.PmsInterval, 5, 8),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenPremenstrualSyndromeOptionIsSevere_ComputesPmsDatesWithinExpectedRangeAndText()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();
            DateTime cycleStart = DateTime.Today;

            viewModel.CalculatePeriodTracker(cycleStart, 28, 5, 3);

            DateTime nextCycleStart = viewModel.CurrentBeginningPeriodDate.AddDays(28);
            double daysBeforeNextPeriod = (nextCycleStart - viewModel.CurrentBeginningPmsDate).TotalDays;

            Assert.That(
                MatchesPmsComputation(daysBeforeNextPeriod, viewModel.PmsInterval, 8, 15),
                Is.True);
        }

        [Test]
        public void CalculatePeriodTracker_WhenCalculatedInsideMenstrualPhase_IsInMenstrualPhaseIsTrue()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();

            viewModel.CalculatePeriodTracker(DateTime.Today.AddDays(-1), 28, 5, 0);

            Assert.That(viewModel.IsInMenstrualPhase, Is.True);
        }

        [Test]
        public void UpdatePeriodTracker_WhenMovedAwayFromMenstrualPhase_IsInMenstrualPhaseCanBecomeFalse()
        {
            CalendarsViewModel viewModel = new CalendarsViewModel();

            viewModel.CalculatePeriodTracker(DateTime.Today, 28, 5, 0);
            viewModel.UpdatePeriodTracker(true);

            Assert.That(viewModel.IsInMenstrualPhase, Is.False);
        }

        private static bool CurrentCycleContainsToday(DateTime currentBeginningPeriodDate, int cycleDays)
        {
            return currentBeginningPeriodDate <= DateTime.Today
                && currentBeginningPeriodDate.AddDays(cycleDays) > DateTime.Today;
        }

        private static bool IsComputedLowFertilityInterval(string interval)
        {
            return interval.StartsWith("Low Fertility Days: ")
                && !interval.Contains("No such days");
        }

        private static bool MatchesPmsComputation(
            double daysBeforeNextPeriod,
            string pmsInterval,
            int minimumDaysBeforeNextPeriod,
            int maximumDaysBeforeNextPeriod)
        {
            return daysBeforeNextPeriod >= minimumDaysBeforeNextPeriod
                && daysBeforeNextPeriod <= maximumDaysBeforeNextPeriod
                && pmsInterval.StartsWith("PMS Days: ");
        }
    }
}