using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.Logic
{
    [TestFixture]
    public class PeriodTrackerServiceTests
    {
        private Mock<IUsersRepository> usersRepositoryMock = null!;
        private Mock<ICurrentUserService> currentUserServiceMock = null!;
        private PeriodTrackerService service = null!;

        [SetUp]
        public void SetUp()
        {
            usersRepositoryMock = new Mock<IUsersRepository>();
            currentUserServiceMock = new Mock<ICurrentUserService>();
            service = new PeriodTrackerService(usersRepositoryMock.Object, currentUserServiceMock.Object);
        }

        [Test]
        public void GetTrackerState_WhenCurrentUserIsNull_ReturnsDefaultState()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            PeriodTrackerState result = service.GetTrackerState();

            Assert.That(DefaultTrackerStateMatches(result), Is.True);
        }

        [Test]
        public void GetTrackerState_WhenCurrentUserExists_ReturnsMappedState()
        {
            User user = CreateUser();
            user.SetPeriodTracker(new DateOnly(2026, 4, 1), 30, 6, 2);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);

            PeriodTrackerState result = service.GetTrackerState();

            Assert.That(MatchesTrackerState(result, new DateTime(2026, 4, 1), 30, 6, 2, true), Is.True);
        }

        [Test]
        public void GetNotes_WhenCurrentUserIsNull_ReturnsEmptyDictionary()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            Dictionary<int, Tuple<string, bool>> result = service.GetNotes();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetNotes_WhenCurrentUserHasNotes_ReturnsSameDictionary()
        {
            User user = CreateUser();
            user.AddPeriodNote(1, "Test note", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            Dictionary<int, Tuple<string, bool>> result = service.GetNotes();

            Assert.That(NoteDictionaryMatches(result, 1, "Test note", false), Is.True);
        }

        [Test]
        public void GetMaxNoteId_WhenUserHasNoNotes_ReturnsZero()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            int result = service.GetMaxNoteId();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_WhenUserHasNotes_ReturnsMaximumIdentifier()
        {
            User user = CreateUser();
            user.AddPeriodNote(2, "Second", false);
            user.AddPeriodNote(7, "Seventh", true);
            user.AddPeriodNote(4, "Fourth", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            int result = service.GetMaxNoteId();

            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        public void UpdatePeriodTracker_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 5)), 29, 6, 1);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void UpdatePeriodTracker_WhenCurrentUserExists_UpdatesUserAndPersists()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 5)), 29, 6, 1);

            Assert.That(UserTrackerMatches(user, new DateOnly(2026, 4, 5), 29, 6, 1), Is.True);
        }

        [Test]
        public void UpdatePeriodTracker_WhenCurrentUserExists_PersistsUser()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 5)), 29, 6, 1);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void AddNote_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.AddNote("Note");

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void AddNote_WhenNoteBodyIsNull_AddsEmptyNoteBody()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.AddNote(null!);

            Assert.That(NoteDictionaryMatches(user.PeriodNotes, 1, string.Empty, false), Is.True);
        }

        [Test]
        public void AddNote_WhenNoteBodyIsNull_PersistsUser()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.AddNote(null!);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void AddNote_WhenUserAlreadyHasNotes_UsesNextIdentifier()
        {
            User user = CreateUser();
            user.AddPeriodNote(3, "Existing", false);
            user.AddPeriodNote(8, "Highest", true);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.AddNote("New note");

            Assert.That(NoteDictionaryMatches(user.PeriodNotes, 9, "New note", false), Is.False);
        }

        [Test]
        public void UpdateNote_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.UpdateNote(1, "Updated", true);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void UpdateNote_WhenNoteBodyIsNull_ReplacesWithEmptyString()
        {
            User user = CreateUser();
            user.AddPeriodNote(5, "Initial", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.UpdateNote(5, null!, true);

            Assert.That(NoteDictionaryMatches(user.PeriodNotes, 5, string.Empty, true), Is.True);
        }

        [Test]
        public void UpdateNote_WhenNoteBodyIsNull_PersistsUser()
        {
            User user = CreateUser();
            user.AddPeriodNote(5, "Initial", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.UpdateNote(5, null!, true);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void DeleteNote_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.DeleteNote(1);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void DeleteNote_WhenNoteDoesNotExist_DoesNothing()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.DeleteNote(999);

            Assert.That(user.PeriodNotes, Is.Empty);
        }

        [Test]
        public void DeleteNote_WhenNoteDoesNotExist_DoesNotPersistUser()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.DeleteNote(999);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void DeleteNote_WhenNoteExists_RemovesNote()
        {
            User user = CreateUser();
            user.AddPeriodNote(2, "Delete me", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.DeleteNote(2);

            Assert.That(user.PeriodNotes.ContainsKey(2), Is.False);
        }

        [Test]
        public void DeleteNote_WhenNoteExists_PersistsUser()
        {
            User user = CreateUser();
            user.AddPeriodNote(2, "Delete me", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.DeleteNote(2);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void SaveCurrentUser_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.SaveCurrentUser();

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void SaveCurrentUser_WhenCurrentUserExists_PersistsUser()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.SaveCurrentUser();

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void GetCurrentUser_WhenCurrentUserExists_ReturnsSameInstance()
        {
            User user = CreateUser();
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            User result = service.GetCurrentUser();

            Assert.That(result, Is.SameAs(user));
        }

        [Test]
        public void GetTrackerState_WhenUserHasNoConfiguredStartDate_UsesTodayAsStartDate()
        {
            User user = CreateUser();
            user.CycleDays = 31;
            user.PeriodLasts = 7;
            user.PMSOption = 2;

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);

            PeriodTrackerState result = service.GetTrackerState();

            Assert.That(MatchesTrackerState(result, DateTime.Today, 31, 7, 2, true), Is.True);
        }

        private static bool DefaultTrackerStateMatches(PeriodTrackerState state)
        {
            return state.HasPeriodTracker == false
                && state.CycleDays == 28
                && state.PeriodLasts == 5
                && state.PmsOption == 0
                && state.StartPeriodDate.Date == DateTime.Today;
        }

        private static bool MatchesTrackerState(
            PeriodTrackerState state,
            DateTime expectedStartDate,
            int expectedCycleDays,
            int expectedPeriodLasts,
            int expectedPmsOption,
            bool expectedHasPeriodTracker)
        {
            return state.StartPeriodDate.Date == expectedStartDate.Date
                && state.CycleDays == expectedCycleDays
                && state.PeriodLasts == expectedPeriodLasts
                && state.PmsOption == expectedPmsOption
                && state.HasPeriodTracker == expectedHasPeriodTracker;
        }

        private static bool NoteDictionaryMatches(
            Dictionary<int, Tuple<string, bool>> notes,
            int noteId,
            string expectedBody,
            bool expectedIsCompleted)
        {
            return notes.Count == 1
                && notes.ContainsKey(noteId)
                && notes[noteId].Item1 == expectedBody
                && notes[noteId].Item2 == expectedIsCompleted;
        }

        private static bool UserTrackerMatches(
            User user,
            DateOnly expectedStartDate,
            int expectedCycleDays,
            int expectedPeriodLasts,
            int expectedPmsOption)
        {
            return user.StartPeriodDate == expectedStartDate
                && user.CycleDays == expectedCycleDays
                && user.PeriodLasts == expectedPeriodLasts
                && user.PMSOption == expectedPmsOption;
        }

        private static User CreateUser()
        {
            return new User(
                1,
                "user@test.com",
                "0700000000",
                "hash",
                false,
                false,
                "user",
                false,
                0);
        }
    }
}