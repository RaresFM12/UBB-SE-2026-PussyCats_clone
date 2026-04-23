using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;
using Moq;

namespace PharmacyApp.Tests.Integration.FeaturesIntegration.PeriodTracker
{
    [TestFixture]
    public class PeriodTrackerServiceIntegrationTests
    {
        [Test]
        public void UpdatePeriodTracker_WhenCalled_UpdatesRealUserState()
        {
            User user = CreateUser();
            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 10)), 30, 7, 2);

            Assert.That(
                UserTrackerMatches(user, new DateOnly(2026, 4, 10), 30, 7, 2),
                Is.True);
        }

        [Test]
        public void UpdatePeriodTracker_WhenCalled_PersistsThroughRepository()
        {
            User user = CreateUser();
            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 10)), 30, 7, 2);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void AddUpdateDeleteNote_WhenCalled_LeavesUserWithoutNotes()
        {
            User user = CreateUser();
            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            service.AddNote("First");
            service.UpdateNote(1, "Updated", true);
            service.DeleteNote(1);

            Assert.That(user.PeriodNotes.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddUpdateDeleteNote_WhenCalled_PersistsUserAcrossFullFlow()
        {
            User user = CreateUser();
            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            service.AddNote("First");
            service.UpdateNote(1, "Updated", true);
            service.DeleteNote(1);

            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Exactly(3));
        }

        [Test]
        public void GetTrackerState_WhenRepositoryReportsConfiguredTracker_ReturnsRealMappedState()
        {
            User user = CreateUser();
            user.SetPeriodTracker(new DateOnly(2026, 3, 20), 31, 6, 1);

            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            PeriodTrackerState state = service.GetTrackerState();

            Assert.That(
                TrackerStateMatches(state, new DateTime(2026, 3, 20), 31, 6, 1, true),
                Is.True);
        }

        private static bool UserTrackerMatches(
            User user,
            DateOnly expectedStartDate,
            int expectedCycleDays,
            int expectedPeriodLasts,
            int expectedPremenstrualSyndromeOption)
        {
            return user.StartPeriodDate == expectedStartDate
                && user.CycleDays == expectedCycleDays
                && user.PeriodLasts == expectedPeriodLasts
                && user.PremenstrualSyndromeOption == expectedPremenstrualSyndromeOption;
        }

        private static bool TrackerStateMatches(
            PeriodTrackerState state,
            DateTime expectedStartDate,
            int expectedCycleDays,
            int expectedPeriodLasts,
            int expectedPremenstrualSyndromeOption,
            bool expectedHasPeriodTracker)
        {
            return state.StartPeriodDate.Date == expectedStartDate.Date
                && state.CycleDays == expectedCycleDays
                && state.PeriodLasts == expectedPeriodLasts
                && state.PremenstrualSyndromeOption == expectedPremenstrualSyndromeOption
                && state.HasPeriodTracker == expectedHasPeriodTracker;
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