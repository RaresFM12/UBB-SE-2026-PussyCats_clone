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

            Assert.That(result.HasPeriodTracker, Is.False);
            Assert.That(result.CycleDays, Is.EqualTo(28));
            Assert.That(result.PeriodLasts, Is.EqualTo(5));
            Assert.That(result.PmsOption, Is.EqualTo(0));
            Assert.That(result.StartPeriodDate.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public void GetTrackerState_WhenCurrentUserExists_ReturnsMappedState()
        {
            User user = CreateUser();
            user.SetPeriodTracker(new DateOnly(2026, 4, 1), 30, 6, 2);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);

            PeriodTrackerState result = service.GetTrackerState();

            Assert.That(result.StartPeriodDate.Date, Is.EqualTo(new DateTime(2026, 4, 1)));
            Assert.That(result.CycleDays, Is.EqualTo(30));
            Assert.That(result.PeriodLasts, Is.EqualTo(6));
            Assert.That(result.PmsOption, Is.EqualTo(2));
            Assert.That(result.HasPeriodTracker, Is.True);
        }

        [Test]
        public void GetNotes_WhenCurrentUserIsNull_ReturnsEmptyDictionary()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            Dictionary<int, Tuple<string, bool>> result = service.GetNotes();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNotes_WhenCurrentUserHasNotes_ReturnsSameDictionary()
        {
            User user = CreateUser();
            user.AddPeriodNote(1, "Test note", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            Dictionary<int, Tuple<string, bool>> result = service.GetNotes();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[1].Item1, Is.EqualTo("Test note"));
            Assert.That(result[1].Item2, Is.False);
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

            Assert.That(user.StartPeriodDate, Is.EqualTo(new DateOnly(2026, 4, 5)));
            Assert.That(user.CycleDays, Is.EqualTo(29));
            Assert.That(user.PeriodLasts, Is.EqualTo(6));
            Assert.That(user.PMSOption, Is.EqualTo(1));
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

            Assert.That(user.PeriodNotes.Count, Is.EqualTo(1));
            Assert.That(user.PeriodNotes[1].Item1, Is.EqualTo(string.Empty));
            Assert.That(user.PeriodNotes[1].Item2, Is.False);
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

            Assert.That(user.PeriodNotes.ContainsKey(9), Is.True);
            Assert.That(user.PeriodNotes[9].Item1, Is.EqualTo("New note"));
            Assert.That(user.PeriodNotes[9].Item2, Is.False);
        }

        [Test]
        public void UpdateNote_WhenCurrentUserIsNull_DoesNothing()
        {
            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns((User)null!);

            service.UpdateNote(1, "Updated", true);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void UpdateNote_WhenNoteBodyIsNull_ReplacesWithEmptyStringAndPersists()
        {
            User user = CreateUser();
            user.AddPeriodNote(5, "Initial", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.UpdateNote(5, null!, true);

            Assert.That(user.PeriodNotes[5].Item1, Is.EqualTo(string.Empty));
            Assert.That(user.PeriodNotes[5].Item2, Is.True);
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

            Assert.That(user.PeriodNotes.Count, Is.EqualTo(0));
            usersRepositoryMock.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void DeleteNote_WhenNoteExists_RemovesNoteAndPersists()
        {
            User user = CreateUser();
            user.AddPeriodNote(2, "Delete me", false);

            currentUserServiceMock.Setup(serviceMock => serviceMock.CurrentUser).Returns(user);

            service.DeleteNote(2);

            Assert.That(user.PeriodNotes.ContainsKey(2), Is.False);
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

            Assert.That(result.StartPeriodDate.Date, Is.EqualTo(DateTime.Today));
            Assert.That(result.CycleDays, Is.EqualTo(31));
            Assert.That(result.PeriodLasts, Is.EqualTo(7));
            Assert.That(result.PmsOption, Is.EqualTo(2));
            Assert.That(result.HasPeriodTracker, Is.True);
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
