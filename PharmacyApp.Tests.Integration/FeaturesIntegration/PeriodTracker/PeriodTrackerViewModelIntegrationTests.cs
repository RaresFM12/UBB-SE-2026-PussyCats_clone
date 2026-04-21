using Microsoft.UI.Xaml;
using Moq;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Features.Period_Tracker.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Integration.FeaturesIntegration.PeriodTracker
{
    [TestFixture]
    public class PeriodTrackerViewModelIntegrationTests
    {
        [Test]
        public void Constructor_WhenTrackerExistsAndTodayIsInMenstrualPhase_BuildsItemsAndUsesExtraDiscountOnAddToBasket()
        {
            User user = CreateUser();
            user.SetPeriodTracker(DateOnly.FromDateTime(DateTime.Today), 28, 5, 0);

            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrderService> orderServiceMock = new Mock<IOrderService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);

            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(new List<Item>
            {
                new Item(11, "Tea", "Producer", "wellness", 100f, 1, label: "", description: "", imagePath: "..\\..\\Assets\\placeholder.png", discount: 0f),
                new Item(12, "Cream", "Producer", "wellness", 50f, 1, label: "", description: "", imagePath: "..\\..\\Assets\\placeholder.png", discount: 0f)
            });

            IPeriodTrackerService periodTrackerService = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);
            IWellnessItemsService wellnessItemsService = new WellnessItemsService(itemsRepositoryMock.Object);
            IBasketService basketService = new BasketService(orderServiceMock.Object);

            PeriodTrackerViewModel viewModel = new PeriodTrackerViewModel(
                periodTrackerService,
                wellnessItemsService,
                basketService);

            Assert.That(viewModel.CalendarsVisibility, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.ShopVisibility, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.ItemsLists.Count, Is.EqualTo(1));
            Assert.That(viewModel.ItemsLists[0].Items.Count, Is.EqualTo(2));

            viewModel.ItemsLists[0].Items[0].AddToBasketCommand.Execute(null);

            orderServiceMock.Verify(service => service.AddToBasket(11, 1, 20f), Times.Once);
        }

        [Test]
        public void AddNoteFlow_WhenCommandAndEditAndDeleteAreExecuted_PersistsThroughRealService()
        {
            User user = CreateUser();
            user.SetPeriodTracker(DateOnly.FromDateTime(DateTime.Today), 28, 5, 0);

            Mock<IUsersRepository> usersRepositoryMock = new Mock<IUsersRepository>();
            Mock<ICurrentUserService> currentUserServiceMock = new Mock<ICurrentUserService>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrderService> orderServiceMock = new Mock<IOrderService>();

            currentUserServiceMock.Setup(service => service.CurrentUser).Returns(user);
            usersRepositoryMock.Setup(repository => repository.UserHasPeriodTracker(user.Id)).Returns(true);
            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            IPeriodTrackerService periodTrackerService = new PeriodTrackerService(
                usersRepositoryMock.Object,
                currentUserServiceMock.Object);
            IWellnessItemsService wellnessItemsService = new WellnessItemsService(itemsRepositoryMock.Object);
            IBasketService basketService = new BasketService(orderServiceMock.Object);

            PeriodTrackerViewModel viewModel = new PeriodTrackerViewModel(
                periodTrackerService,
                wellnessItemsService,
                basketService);

            viewModel.AddNoteCommand.Execute(null);
            viewModel.Notes[0].NoteBody = "Updated note";
            viewModel.Notes[0].NoteIsDone = true;
            viewModel.Notes[0].DeleteNoteCommand.Execute(null);

            Assert.That(user.PeriodNotes.Count, Is.EqualTo(0));
            usersRepositoryMock.Verify(repository => repository.UpdateUser(user), Times.AtLeastOnce);
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
