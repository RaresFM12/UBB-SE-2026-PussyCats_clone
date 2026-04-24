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

        [Test]
        public void RefreshItems_ClearsAndReloadsFromService()
        {
            var vm = CreateViewModel(items: new List<Item> { CreateItem() });
            mockAdminService.Setup(s => s.GetAllItems()).Returns(new List<Item> { CreateItem(2), CreateItem(3) });

            vm.RefreshItems();

            Assert.AreEqual(2, vm.Items.Count);
        }

        [Test]
        public void RefreshSubstances_ClearsAndReloadsFromService()
        {
            var vm = CreateViewModel(substances: new List<Substance> { CreateSubstance() });
            mockAdminService.Setup(s => s.GetAllSubstances()).Returns(new List<Substance> { CreateSubstance("A"), CreateSubstance("B"), CreateSubstance("C") });

            vm.RefreshSubstances();

            Assert.AreEqual(3, vm.Substances.Count);
        }

        [Test]
        public void ToggleAddSubstanceGrid_WhenCollapsed_SetsVisible()
        {
            var vm = CreateViewModel();
            vm.AddSubstanceGridVisibility = Visibility.Collapsed;

            vm.ToggleAddSubstanceGrid();

            Assert.AreEqual(Visibility.Visible, vm.AddSubstanceGridVisibility);
        }

        [Test]
        public void ToggleAddSubstanceGrid_WhenVisible_SetsCollapsed()
        {
            var vm = CreateViewModel();
            vm.AddSubstanceGridVisibility = Visibility.Visible;

            vm.ToggleAddSubstanceGrid();

            Assert.AreEqual(Visibility.Collapsed, vm.AddSubstanceGridVisibility);
        }

        [Test]
        public void ToggleAddSubstanceGrid_WhenOpened_CollapsesUpdateGrid()
        {
            var vm = CreateViewModel();
            vm.UpdateSubstanceGridVisibility = Visibility.Visible;
            vm.AddSubstanceGridVisibility = Visibility.Collapsed;

            vm.ToggleAddSubstanceGrid();

            Assert.AreEqual(Visibility.Collapsed, vm.UpdateSubstanceGridVisibility);
        }

        [Test]
        public void ToggleUpdateSubstanceGrid_WhenCollapsed_SetsVisible()
        {
            var vm = CreateViewModel();
            vm.UpdateSubstanceGridVisibility = Visibility.Collapsed;

            vm.ToggleUpdateSubstanceGrid();

            Assert.AreEqual(Visibility.Visible, vm.UpdateSubstanceGridVisibility);
        }

        [Test]
        public void ToggleUpdateSubstanceGrid_WhenVisible_SetsCollapsed()
        {
            var vm = CreateViewModel();
            vm.UpdateSubstanceGridVisibility = Visibility.Visible;

            vm.ToggleUpdateSubstanceGrid();

            Assert.AreEqual(Visibility.Collapsed, vm.UpdateSubstanceGridVisibility);
        }

        [Test]
        public void ToggleUpdateSubstanceGrid_WhenOpened_CollapsesAddGrid()
        {
            var vm = CreateViewModel();
            vm.AddSubstanceGridVisibility = Visibility.Visible;
            vm.UpdateSubstanceGridVisibility = Visibility.Collapsed;

            vm.ToggleUpdateSubstanceGrid();

            Assert.AreEqual(Visibility.Collapsed, vm.AddSubstanceGridVisibility);
        }

        [Test]
        public void TryValidateActiveSubstance_DelegatesToService()
        {
            var vm = CreateViewModel();
            var existing = new Dictionary<string, float>();
            string outError;
            mockAdminService
                .Setup(s => s.TryValidateActiveSubstance("Aspirin", 50f, existing, out outError))
                .Returns(true);

            bool result = vm.TryValidateActiveSubstance("Aspirin", 50f, existing, out string error);

            Assert.IsTrue(result);
            mockAdminService.Verify(s => s.TryValidateActiveSubstance("Aspirin", 50f, existing, out outError), Times.Once);
        }

        [Test]
        public void TryValidateActiveSubstance_WhenServiceReturnsFalse_ReturnsFalseWithMessage()
        {
            var vm = CreateViewModel();
            var existing = new Dictionary<string, float>();
            string outError = "Substance already exists for this item";
            mockAdminService
                .Setup(s => s.TryValidateActiveSubstance("Aspirin", 50f, existing, out outError))
                .Returns(false);

            bool result = vm.TryValidateActiveSubstance("Aspirin", 50f, existing, out string error);

            Assert.IsFalse(result);
            Assert.AreEqual("Substance already exists for this item", error);
        }

        [Test]
        public void TryValidateBatch_DelegatesToService()
        {
            var vm = CreateViewModel();
            var expiry = new DateOnly(2027, 1, 1);
            var today = new DateOnly(2026, 1, 1);
            string outError;
            mockAdminService
                .Setup(s => s.TryValidateBatch(expiry, 10, today, out outError))
                .Returns(true);

            bool result = vm.TryValidateBatch(expiry, 10, today, out string error);

            Assert.IsTrue(result);
            mockAdminService.Verify(s => s.TryValidateBatch(expiry, 10, today, out outError), Times.Once);
        }

        [Test]
        public void TryValidateBatch_WhenServiceReturnsFalse_ReturnsFalseWithMessage()
        {
            var vm = CreateViewModel();
            var expiry = new DateOnly(2020, 1, 1);
            var today = new DateOnly(2026, 1, 1);
            string outError = "expiration date must be later than current date";
            mockAdminService
                .Setup(s => s.TryValidateBatch(expiry, 10, today, out outError))
                .Returns(false);

            bool result = vm.TryValidateBatch(expiry, 10, today, out string error);

            Assert.IsFalse(result);
            Assert.AreEqual("expiration date must be later than current date", error);
        }

        [Test]
        public void GetItemById_DelegatesToService()
        {
            var vm = CreateViewModel();
            var item = CreateItem(42);
            mockAdminService.Setup(s => s.GetItemById(42)).Returns(item);

            var result = vm.GetItemById(42);

            Assert.AreEqual(item, result);
        }

        [Test]
        public void GetSubstanceByName_DelegatesToService()
        {
            var vm = CreateViewModel();
            var substance = CreateSubstance("Ibuprofen");
            mockAdminService.Setup(s => s.GetSubstanceByName("Ibuprofen")).Returns(substance);

            var result = vm.GetSubstanceByName("Ibuprofen");

            Assert.AreEqual(substance, result);
        }

        [Test]
        public void SubstanceExists_DelegatesToService()
        {
            var vm = CreateViewModel();
            mockAdminService.Setup(s => s.SubstanceExists("Aspirin")).Returns(true);

            bool result = vm.SubstanceExists("Aspirin");

            Assert.IsTrue(result);
            mockAdminService.Verify(s => s.SubstanceExists("Aspirin"), Times.Once);
        }

        [Test]
        public void UpdateItemById_CallsService()
        {
            var vm = CreateViewModel();
            var item = CreateItem(7);

            vm.UpdateItemById(7, item);

            mockAdminService.Verify(s => s.UpdateItemById(7, item), Times.Once);
        }

        [Test]
        public void UpdateSubstanceByName_CallsService()
        {
            var vm = CreateViewModel();
            var substance = CreateSubstance("Ibuprofen");

            vm.UpdateSubstanceByName("Ibuprofen", substance);

            mockAdminService.Verify(s => s.UpdateSubstanceByName("Ibuprofen", substance), Times.Once);
        }

        [Test]
        public void RemoveSubstanceByName_CallsService()
        {
            var vm = CreateViewModel();
            var substance = CreateSubstance("Ibuprofen");

            vm.RemoveSubstanceByName(substance);

            mockAdminService.Verify(s => s.RemoveSubstanceByName(substance), Times.Once);
        }

        [Test]
        public void AddItemFromDetails_CallsService()
        {
            var vm = CreateViewModel();
            var substances = new Dictionary<string, float> { { "Aspirin", 10f } };
            var batches = new Dictionary<DateOnly, int> { { new DateOnly(2027, 1, 1), 5 } };

            vm.AddItemFromDetails("Name", "Producer", "Category", 10f, 20, "label", "desc", "img.png", 0f, substances, batches);

            mockAdminService.Verify(s => s.AddItemFromDetails(
                "Name", "Producer", "Category", 10f, 20, "label", "desc", "img.png", 0f, substances, batches), Times.Once);
        }

        [Test]
        public void UpdateItemFromDetails_CallsService()
        {
            var vm = CreateViewModel();
            var substances = new Dictionary<string, float> { { "Aspirin", 10f } };
            var batches = new Dictionary<DateOnly, int> { { new DateOnly(2027, 1, 1), 5 } };

            vm.UpdateItemFromDetails(1, "Name", "Producer", "Category", 10f, 20, "label", "desc", "img.png", 0f, substances, batches);

            mockAdminService.Verify(s => s.UpdateItemFromDetails(
                1, "Name", "Producer", "Category", 10f, 20, "label", "desc", "img.png", 0f, substances, batches), Times.Once);
        }
    }
}