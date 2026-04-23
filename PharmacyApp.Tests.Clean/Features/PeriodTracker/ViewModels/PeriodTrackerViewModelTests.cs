using Microsoft.UI.Xaml;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Features.Period_Tracker.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.ViewModels
{
    [TestFixture]
    public class PeriodTrackerViewModelTests
    {
        private Mock<IPeriodTrackerService> periodTrackerServiceMock = null!;
        private Mock<IWellnessItemsService> wellnessItemsServiceMock = null!;
        private Mock<IBasketService> basketServiceMock = null!;

        [SetUp]
        public void SetUp()
        {
            periodTrackerServiceMock = new Mock<IPeriodTrackerService>();
            wellnessItemsServiceMock = new Mock<IWellnessItemsService>();
            basketServiceMock = new Mock<IBasketService>();

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>());

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item>());
        }

        [Test]
        public void Constructor_WhenTrackerDoesNotExist_HidesCalendarsAndShop()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today,
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = false
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.That(
                MatchesVisibilityState(
                    viewModel,
                    Visibility.Collapsed,
                    Visibility.Collapsed),
                Is.True);
        }

        [Test]
        public void Constructor_WhenTrackerExists_LoadsInitialStateAndBuildsItemRows()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today,
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = true
                });

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item>
                {
                    CreateItem(1), CreateItem(2), CreateItem(3), CreateItem(4), CreateItem(5)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.That(
                MatchesTrackerAndItemsState(
                    viewModel,
                    Visibility.Visible,
                    Visibility.Visible,
                    2,
                    4,
                    1),
                Is.True);
        }

        [Test]
        public void Constructor_WhenTrackerExistsButNoItems_HidesShop()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today,
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = true
                });

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item>());

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.That(
                MatchesTrackerWithoutItemsState(viewModel),
                Is.True);
        }

        [Test]
        public void Constructor_WhenNotesExist_LoadsAtMostFourNotesOrderedByIdentifier()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [5] = Tuple.Create("Five", false),
                    [1] = Tuple.Create("One", true),
                    [3] = Tuple.Create("Three", false),
                    [2] = Tuple.Create("Two", true),
                    [4] = Tuple.Create("Four", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.That(
                NotesMatch(viewModel.Notes, 1, 2, 3, 4),
                Is.True);
        }

        [Test]
        public void CalculateCommand_WhenExecuted_UpdatesService()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            PeriodTrackerViewModel viewModel = CreateViewModel();
            viewModel.StartPeriodDate = new DateTimeOffset(new DateTime(2026, 4, 1));
            viewModel.CycleDaysInput = 29;
            viewModel.PeriodLastsInput = 6;
            viewModel.PremenstrualSyndromeOptionInput = 1;

            viewModel.CalculateCommand.Execute(null);

            periodTrackerServiceMock.Verify(
                service => service.UpdatePeriodTracker(viewModel.StartPeriodDate, 29, 6, 1),
                Times.Once);
        }

        [Test]
        public void CalculateCommand_WhenExecuted_ShowsCalendars()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            PeriodTrackerViewModel viewModel = CreateViewModel();
            viewModel.StartPeriodDate = new DateTimeOffset(new DateTime(2026, 4, 1));
            viewModel.CycleDaysInput = 29;
            viewModel.PeriodLastsInput = 6;
            viewModel.PremenstrualSyndromeOptionInput = 1;

            viewModel.CalculateCommand.Execute(null);

            Assert.That(viewModel.CalendarsVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void AddNoteCommand_WhenNotesAreBelowMaximum_AddsNote()
        {
            periodTrackerServiceMock
                .SetupSequence(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [1] = Tuple.Create(string.Empty, false)
                });

            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.AddNoteCommand.Execute(null);

            periodTrackerServiceMock.Verify(service => service.AddNote(string.Empty), Times.Once);
        }

        [Test]
        public void AddNoteCommand_WhenNotesAreBelowMaximum_ReloadsNotes()
        {
            periodTrackerServiceMock
                .SetupSequence(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [1] = Tuple.Create(string.Empty, false)
                });

            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.AddNoteCommand.Execute(null);

            Assert.That(viewModel.Notes.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddNoteCommand_WhenNotesAlreadyAtMaximum_DoesNotAddAnotherNote()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [1] = Tuple.Create("One", false),
                    [2] = Tuple.Create("Two", false),
                    [3] = Tuple.Create("Three", false),
                    [4] = Tuple.Create("Four", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.AddNoteCommand.Execute(null);

            periodTrackerServiceMock.Verify(service => service.AddNote(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AddNoteCommand_WhenNotesAlreadyAtMaximum_UpdatesAddNoteAvailability()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [1] = Tuple.Create("One", false),
                    [2] = Tuple.Create("Two", false),
                    [3] = Tuple.Create("Three", false),
                    [4] = Tuple.Create("Four", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.AddNoteCommand.Execute(null);

            Assert.That(
                MatchesAddNoteUnavailableState(viewModel),
                Is.True);
        }

        [Test]
        public void LoadedNote_WhenBodyChanges_PersistsUpdatedNote()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [7] = Tuple.Create("Initial", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.Notes[0].NoteBody = "Updated";

            periodTrackerServiceMock.Verify(service => service.UpdateNote(7, "Updated", false), Times.Once);
        }

        [Test]
        public void LoadedNoteDeleteCommand_WhenExecuted_DeletesNote()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .SetupSequence(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [7] = Tuple.Create("Initial", false)
                })
                .Returns(new Dictionary<int, Tuple<string, bool>>());

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.Notes[0].DeleteNoteCommand.Execute(null);

            periodTrackerServiceMock.Verify(service => service.DeleteNote(7), Times.Once);
        }

        [Test]
        public void LoadedNoteDeleteCommand_WhenExecuted_ReloadsNotes()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .SetupSequence(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [7] = Tuple.Create("Initial", false)
                })
                .Returns(new Dictionary<int, Tuple<string, bool>>());

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.Notes[0].DeleteNoteCommand.Execute(null);

            Assert.That(viewModel.Notes.Count, Is.EqualTo(0));
        }

        [Test]
        public void NextCycleCommand_WhenCalendarsAreHidden_DoesNothing()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            PeriodTrackerViewModel viewModel = CreateViewModel();
            int initialRows = viewModel.ItemsLists.Count;

            viewModel.NextCycleCommand.Execute(null);

            Assert.That(viewModel.ItemsLists.Count, Is.EqualTo(initialRows));
        }

        [Test]
        public void PreviousCycleCommand_WhenCalendarsAreVisible_RebuildsItemsWithoutThrowing()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today,
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = true
                });

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item> { CreateItem(1), CreateItem(2) });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.DoesNotThrow(() => viewModel.PreviousCycleCommand.Execute(null));
        }

        [Test]
        public void PreviousCycleCommand_WhenCalendarsAreVisible_RebuildsItemsState()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today,
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = true
                });

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item> { CreateItem(1), CreateItem(2) });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.PreviousCycleCommand.Execute(null);

            Assert.That(
                MatchesPreviousCycleItemsState(viewModel),
                Is.True);
        }

        [Test]
        public void Constructor_WhenTrackerExistsOutsideMenstrualPhase_BuildsItemsWithNoExtraDiscount()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState
                {
                    StartPeriodDate = DateTime.Today.AddDays(-10),
                    CycleDays = 28,
                    PeriodLasts = 5,
                    PremenstrualSyndromeOption = 0,
                    HasPeriodTracker = true
                });

            wellnessItemsServiceMock
                .Setup(service => service.GetWellnessItems())
                .Returns(new List<Item> { CreateItem(1) });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.ItemsLists[0].Items[0].AddToBasketCommand.Execute(null);

            basketServiceMock.Verify(service => service.AddToBasket(1, 1, 0f), Times.Once);
        }

        [Test]
        public void LoadedNote_WhenIsDoneChanges_PersistsUpdatedNote()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [7] = Tuple.Create("Initial", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            viewModel.Notes[0].NoteIsDone = true;

            periodTrackerServiceMock.Verify(service => service.UpdateNote(7, "Initial", true), Times.Once);
        }

        [Test]
        public void Constructor_WhenNotesAreBelowMaximum_CanAddNoteIsTrueAndVisibilityIsVisible()
        {
            periodTrackerServiceMock
                .Setup(service => service.GetTrackerState())
                .Returns(new PeriodTrackerState { HasPeriodTracker = false });

            periodTrackerServiceMock
                .Setup(service => service.GetNotes())
                .Returns(new Dictionary<int, Tuple<string, bool>>
                {
                    [1] = Tuple.Create("One", false)
                });

            PeriodTrackerViewModel viewModel = CreateViewModel();

            Assert.That(
                MatchesAddNoteAvailableState(viewModel),
                Is.True);
        }

        private PeriodTrackerViewModel CreateViewModel()
        {
            return new PeriodTrackerViewModel(
                periodTrackerServiceMock.Object,
                wellnessItemsServiceMock.Object,
                basketServiceMock.Object);
        }

        private static Item CreateItem(int id)
        {
            return new Item(
                id,
                $"Item {id}",
                "Producer",
                "wellness",
                20f,
                1,
                label: "",
                description: "",
                imagePath: "..\\..\\Assets\\placeholder.png",
                discount: 0f);
        }

        private static bool MatchesVisibilityState(
            PeriodTrackerViewModel viewModel,
            Visibility expectedCalendarsVisibility,
            Visibility expectedShopVisibility)
        {
            return viewModel.CalendarsVisibility == expectedCalendarsVisibility
                && viewModel.ShopVisibility == expectedShopVisibility;
        }

        private static bool MatchesTrackerAndItemsState(
            PeriodTrackerViewModel viewModel,
            Visibility expectedCalendarsVisibility,
            Visibility expectedShopVisibility,
            int expectedRowsCount,
            int expectedFirstRowCount,
            int expectedSecondRowCount)
        {
            return viewModel.CalendarsVisibility == expectedCalendarsVisibility
                && viewModel.ShopVisibility == expectedShopVisibility
                && viewModel.ItemsLists.Count == expectedRowsCount
                && viewModel.ItemsLists[0].Items.Count == expectedFirstRowCount
                && viewModel.ItemsLists[1].Items.Count == expectedSecondRowCount;
        }

        private static bool MatchesTrackerWithoutItemsState(PeriodTrackerViewModel viewModel)
        {
            return viewModel.CalendarsVisibility == Visibility.Visible
                && viewModel.ShopVisibility == Visibility.Collapsed
                && viewModel.ItemsLists.Count == 0;
        }

        private static bool NotesMatch(
            IList<NoteViewModel> notes,
            int firstId,
            int secondId,
            int thirdId,
            int fourthId)
        {
            return notes.Count == 4
                && notes[0].NoteId == firstId
                && notes[1].NoteId == secondId
                && notes[2].NoteId == thirdId
                && notes[3].NoteId == fourthId;
        }

        private static bool MatchesAddNoteUnavailableState(PeriodTrackerViewModel viewModel)
        {
            return viewModel.CanAddNote == false
                && viewModel.AddNoteVisibility == Visibility.Collapsed;
        }

        private static bool MatchesPreviousCycleItemsState(PeriodTrackerViewModel viewModel)
        {
            return viewModel.ItemsLists.Count == 1
                && viewModel.ShopVisibility == Visibility.Visible;
        }

        private static bool MatchesAddNoteAvailableState(PeriodTrackerViewModel viewModel)
        {
            return viewModel.CanAddNote
                && viewModel.AddNoteVisibility == Visibility.Visible;
        }
    }
}