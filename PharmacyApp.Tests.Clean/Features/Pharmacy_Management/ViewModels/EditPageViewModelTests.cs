using Microsoft.UI.Xaml;
using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Services;
using PharmacyApp.Features.Pharmacy_Management.ViewModels;
using PharmacyApp.Models;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Unit.Features.PharmacyManagement.ViewModels
{
    [TestFixture]
    public class EditPageViewModelTests
    {
        private Mock<IAdminService> mockAdminService;

        private static Item CreateItem(int id = 1, string name = "Item")
        {
            return new Item(
                id,
                name,
                "Producer",
                "Category",
                10f,
                10,
                quantity: 5);
        }

        private static Substance CreateSubstance(string name = "Sub")
        {
            return new Substance(name, 100f, "desc");
        }

        [SetUp]
        public void Setup()
        {
            mockAdminService = new Mock<IAdminService>();
        }

        private EditPageViewModel CreateViewModel(
            List<Item> items = null,
            List<Substance> substances = null)
        {
            mockAdminService
                .Setup(service => service.GetAllItems())
                .Returns(items ?? new List<Item>());

            mockAdminService
                .Setup(service => service.GetAllSubstances())
                .Returns(substances ?? new List<Substance>());

            return new EditPageViewModel(mockAdminService.Object);
        }

        [Test]
        public void Constructor_LoadsItemsFromService()
        {
            List<Item> items = new List<Item> { CreateItem(), CreateItem(2) };

            EditPageViewModel viewModel = CreateViewModel(items: items);

            Assert.That(viewModel.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_LoadsSubstancesFromService()
        {
            List<Substance> substances = new List<Substance> { CreateSubstance(), CreateSubstance("S2") };

            EditPageViewModel viewModel = CreateViewModel(substances: substances);

            Assert.That(viewModel.Substances.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchItems_LoadsFilteredResults()
        {
            List<Item> results = new List<Item> { CreateItem(10) };

            EditPageViewModel viewModel = CreateViewModel();

            mockAdminService
                .Setup(service => service.SearchItemsByName("Paracetamol"))
                .Returns(results);

            viewModel.SearchItems("Paracetamol");

            Assert.That(viewModel.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShowExpiredItems_LoadsExpiredItemsOnly()
        {
            List<Item> expired = new List<Item> { CreateItem(5), CreateItem(6) };

            EditPageViewModel viewModel = CreateViewModel();

            mockAdminService
                .Setup(service => service.GetExpiredItems())
                .Returns(expired);

            viewModel.ShowExpiredItems();

            Assert.That(viewModel.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void ActivateItemsSection_SetsCorrectVisibilityState()
        {
            EditPageViewModel viewModel = CreateViewModel();

            viewModel.ActivateItemsSection();

            Assert.That(viewModel.ItemListButtonsVisibility, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.ItemBottomButtonsVisibility, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.ShowExpiredItemsToggleVisibility, Is.EqualTo(Visibility.Visible));

            Assert.That(viewModel.SubstanceListButtonsVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(viewModel.SubstanceBottomButtonsVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(viewModel.AddSubstanceGridVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(viewModel.UpdateSubstanceGridVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void ActivateSubstancesSection_SetsCorrectVisibilityState()
        {
            EditPageViewModel viewModel = CreateViewModel();

            viewModel.ActivateSubstancesSection();

            Assert.That(viewModel.ItemListButtonsVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(viewModel.ItemBottomButtonsVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(viewModel.ShowExpiredItemsToggleVisibility, Is.EqualTo(Visibility.Collapsed));

            Assert.That(viewModel.SubstanceListButtonsVisibility, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.SubstanceBottomButtonsVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void AddItemWithQuantity_CallsService()
        {
            EditPageViewModel viewModel = CreateViewModel();
            Item item = CreateItem();

            viewModel.AddItemWithQuantity(item);

            mockAdminService.Verify(service => service.AddItemWithQuantity(item), Times.Once);
        }

        [Test]
        public void RemoveItem_CallsService()
        {
            EditPageViewModel viewModel = CreateViewModel();

            viewModel.RemoveItem(1);

            mockAdminService.Verify(service => service.RemoveItem(1), Times.Once);
        }

        [Test]
        public void AddSubstance_CallsService()
        {
            EditPageViewModel viewModel = CreateViewModel();
            Substance substance = CreateSubstance();

            viewModel.AddSubstance(substance);

            mockAdminService.Verify(service => service.AddSubstance(substance), Times.Once);
        }

        [Test]
        public void AddSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            EditPageViewModel viewModel = CreateViewModel();
            bool raised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(EditPageViewModel.AddSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            viewModel.AddSubstanceGridVisibility = Visibility.Visible;

            Assert.That(raised, Is.True);
        }

        [Test]
        public void UpdateSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            EditPageViewModel viewModel = CreateViewModel();
            bool raised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(EditPageViewModel.UpdateSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            viewModel.UpdateSubstanceGridVisibility = Visibility.Visible;

            Assert.That(raised, Is.True);
        }
    }
}