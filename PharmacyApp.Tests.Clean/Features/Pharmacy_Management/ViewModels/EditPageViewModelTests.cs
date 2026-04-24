using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Services;
using PharmacyApp.Features.Pharmacy_Management.ViewModels;
using PharmacyApp.Models;

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
            mockAdminService.Setup(s => s.GetAllItems())
                .Returns(items ?? new List<Item>());

            mockAdminService.Setup(s => s.GetAllSubstances())
                .Returns(substances ?? new List<Substance>());

            return new EditPageViewModel(mockAdminService.Object);
        }

        [Test]
        public void Constructor_LoadsItemsFromService()
        {
            var items = new List<Item> { CreateItem(), CreateItem(2) };

            var viewModel = CreateViewModel(items: items);

            Assert.That(viewModel.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_LoadsSubstancesFromService()
        {
            var substances = new List<Substance> { CreateSubstance(), CreateSubstance("S2") };

            var viewModel = CreateViewModel(substances: substances);

            Assert.That(viewModel.Substances.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchItems_LoadsFilteredResults()
        {
            var results = new List<Item> { CreateItem(10) };

            var viewModel = CreateViewModel();

            mockAdminService.Setup(substance => substance.SearchItemsByName("Paracetamol"))
                .Returns(results);

            viewModel.SearchItems("Paracetamol");

            Assert.That(viewModel.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShowExpiredItems_LoadsExpiredItemsOnly()
        {
            var expired = new List<Item> { CreateItem(5), CreateItem(6) };

            var viewModel = CreateViewModel();

            mockAdminService.Setup(substance => substance.GetExpiredItems())
                .Returns(expired);

            viewModel.ShowExpiredItems();

            Assert.That(viewModel.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void ActivateItemsSection_SetsCorrectVisibility()
        {
            var viewModel = CreateViewModel();

            viewModel.ActivateItemsSection();
            Assert.That(
                viewModel.ItemListButtonsVisibility == Visibility.Visible &&
                viewModel.ItemBottomButtonsVisibility == Visibility.Visible &&
                viewModel.ShowExpiredItemsToggleVisibility == Visibility.Visible &&
                viewModel.SubstanceListButtonsVisibility == Visibility.Collapsed &&
                viewModel.SubstanceBottomButtonsVisibility == Visibility.Collapsed &&
                viewModel.AddSubstanceGridVisibility == Visibility.Collapsed &&
                viewModel.UpdateSubstanceGridVisibility == Visibility.Collapsed);
        }

        [Test]
        public void ActivateSubstancesSection_SetsCorrectVisibilityState()
        {
            var viewModel = CreateViewModel();

            viewModel.ActivateSubstancesSection();
            Assert.That(viewModel.ItemListButtonsVisibility == Visibility.Collapsed &&
                viewModel.ItemBottomButtonsVisibility == Visibility.Collapsed &&
                viewModel.ShowExpiredItemsToggleVisibility == Visibility.Collapsed &&
                viewModel.SubstanceListButtonsVisibility == Visibility.Visible &&
                viewModel.SubstanceBottomButtonsVisibility == Visibility.Visible);
        }

        [Test]
        public void AddItemWithQuantity_CallsService()
        {
            var viewModel = CreateViewModel();
            var item = CreateItem();

            viewModel.AddItemWithQuantity(item);

            mockAdminService.Verify(s => s.AddItemWithQuantity(item), Times.Once);
        }

        [Test]
        public void RemoveItem_CallsService()
        {
            var viewModel = CreateViewModel();

            viewModel.RemoveItemById(1);

            mockAdminService.Verify(s => s.RemoveItemById(1), Times.Once);
        }

        [Test]
        public void AddSubstance_CallsService()
        {
            var viewModel = CreateViewModel();
            var substance = CreateSubstance();

            viewModel.AddSubstance(substance);

            mockAdminService.Verify(s => s.AddSubstance(substance), Times.Once);
        }

        [Test]
        public void AddSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EditPageViewModel.AddSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            viewModel.AddSubstanceGridVisibility = Visibility.Visible;

            Assert.IsTrue(raised);
        }

        [Test]
        public void UpdateSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EditPageViewModel.UpdateSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            viewModel.UpdateSubstanceGridVisibility = Visibility.Visible;

            Assert.IsTrue(raised);
        }
    }
}