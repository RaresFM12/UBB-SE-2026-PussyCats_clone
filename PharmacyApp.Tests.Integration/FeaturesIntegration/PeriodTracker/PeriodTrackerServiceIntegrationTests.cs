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
        public void UpdatePeriodTracker_WhenCalled_UpdatesRealUserStateAndPersistsThroughRepository()
        {
            User user = CreateUser();
            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);

            PeriodTrackerService service = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);

            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2026, 4, 10)), 30, 7, 2);

            Assert.That(user.StartPeriodDate, Is.EqualTo(new DateOnly(2026, 4, 10)));
            Assert.That(user.CycleDays, Is.EqualTo(30));
            Assert.That(user.PeriodLasts, Is.EqualTo(7));
            Assert.That(user.PMSOption, Is.EqualTo(2));
            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Test]
        public void AddUpdateDeleteNote_WhenCalled_ChangesRealUserNotesAcrossFullFlow()
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

            Assert.That(state.StartPeriodDate.Date, Is.EqualTo(new DateTime(2026, 3, 20)));
            Assert.That(state.CycleDays, Is.EqualTo(31));
            Assert.That(state.PeriodLasts, Is.EqualTo(6));
            Assert.That(state.PmsOption, Is.EqualTo(1));
            Assert.That(state.HasPeriodTracker, Is.True);
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
