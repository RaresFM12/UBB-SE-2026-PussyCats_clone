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

            var vm = CreateViewModel(items: items);

            Assert.AreEqual(2, vm.Items.Count);
        }

        [Test]
        public void Constructor_LoadsSubstancesFromService()
        {
            var substances = new List<Substance> { CreateSubstance(), CreateSubstance("S2") };

            var vm = CreateViewModel(substances: substances);

            Assert.AreEqual(2, vm.Substances.Count);
        }

        [Test]
        public void SearchItems_LoadsFilteredResults()
        {
            var results = new List<Item> { CreateItem(10) };

            var vm = CreateViewModel();

            mockAdminService.Setup(s => s.SearchItemsByName("Paracetamol"))
                .Returns(results);

            vm.SearchItems("Paracetamol");

            Assert.AreEqual(1, vm.Items.Count);
        }

        [Test]
        public void ShowExpiredItems_LoadsExpiredItemsOnly()
        {
            var expired = new List<Item> { CreateItem(5), CreateItem(6) };

            var vm = CreateViewModel();

            mockAdminService.Setup(s => s.GetExpiredItems())
                .Returns(expired);

            vm.ShowExpiredItems();

            Assert.AreEqual(2, vm.Items.Count);
        }

        [Test]
        public void ActivateItemsSection_SetsCorrectVisibility()
        {
            var vm = CreateViewModel();

            vm.ActivateItemsSection();
            Assert.That(
                vm.ItemListButtonsVisibility == Visibility.Visible &&
                vm.ItemBottomButtonsVisibility == Visibility.Visible &&
                vm.ShowExpiredItemsToggleVisibility == Visibility.Visible &&
                vm.SubstanceListButtonsVisibility == Visibility.Collapsed &&
                vm.SubstanceBottomButtonsVisibility == Visibility.Collapsed &&
                vm.AddSubstanceGridVisibility == Visibility.Collapsed &&
                vm.UpdateSubstanceGridVisibility == Visibility.Collapsed);
        }

        [Test]
        public void ActivateSubstancesSection_SetsCorrectVisibilityState()
        {
            var vm = CreateViewModel();

            vm.ActivateSubstancesSection();
            Assert.That(vm.ItemListButtonsVisibility == Visibility.Collapsed &&
                vm.ItemBottomButtonsVisibility == Visibility.Collapsed &&
                vm.ShowExpiredItemsToggleVisibility == Visibility.Collapsed &&
                vm.SubstanceListButtonsVisibility == Visibility.Visible &&
                vm.SubstanceBottomButtonsVisibility == Visibility.Visible);
        }

        [Test]
        public void AddItemWithQuantity_CallsService()
        {
            var vm = CreateViewModel();
            var item = CreateItem();

            vm.AddItemWithQuantity(item);

            mockAdminService.Verify(s => s.AddItemWithQuantity(item), Times.Once);
        }

        [Test]
        public void RemoveItem_CallsService()
        {
            var vm = CreateViewModel();

            vm.RemoveItemById(1);

            mockAdminService.Verify(s => s.RemoveItemById(1), Times.Once);
        }

        [Test]
        public void AddSubstance_CallsService()
        {
            var vm = CreateViewModel();
            var substance = CreateSubstance();

            vm.AddSubstance(substance);

            mockAdminService.Verify(s => s.AddSubstance(substance), Times.Once);
        }

        [Test]
        public void AddSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();
            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EditPageViewModel.AddSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            vm.AddSubstanceGridVisibility = Visibility.Visible;

            Assert.IsTrue(raised);
        }

        [Test]
        public void UpdateSubstanceGridVisibility_Set_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();
            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EditPageViewModel.UpdateSubstanceGridVisibility))
                {
                    raised = true;
                }
            };

            vm.UpdateSubstanceGridVisibility = Visibility.Visible;

            Assert.IsTrue(raised);
        }
    }
}