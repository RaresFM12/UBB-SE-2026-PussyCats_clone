using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PharmacyApp.Tests.Unit.Features.Orders
{
    [TestFixture]
    public class BasketItemViewModelTests
    {
        private BasketItemViewModel CreateItem(
            int id = 1,
            string imagePath = "img.png",
            string name = "Aspirin",
            string producer = "Bayer",
            int quantity = 2,
            float baseDiscount = 0f,
            float extraDiscount = 0f,
            float userDiscount = 0f,
            float price = 10f)
        {
            return new BasketItemViewModel(id, imagePath, name, producer, quantity,
                baseDiscount, extraDiscount, userDiscount, price);
        }

        [Test]
        public void Constructor_NegativeQuantity_ClampsToZero()
        {
            var item = CreateItem(quantity: -5);

            Assert.AreEqual(0, item.ItemQuantityInBasket);
        }

        [Test]
        public void ItemActiveDiscount_WithBaseAndExtra_CombinesCorrectly()
        {
            var item = CreateItem(baseDiscount: 0.1f, extraDiscount: 0.2f);

            // (1 - (1-0.1)*(1-0.2)) = 1 - 0.9*0.8 = 1 - 0.72 = 0.28
            Assert.AreEqual(0.28f, item.ItemActiveDiscount, 0.001f);
        }

        [Test]
        public void ItemActiveDiscount_WithZeroDiscounts_IsZero()
        {
            var item = CreateItem(baseDiscount: 0f, extraDiscount: 0f);

            Assert.AreEqual(0f, item.ItemActiveDiscount, 0.001f);
        }

        [Test]
        public void ItemDiscountString_FormatsAsPercentage()
        {
            var item = CreateItem(baseDiscount: 0.25f, extraDiscount: 0f);

            Assert.AreEqual("-25%", item.ItemDiscountString);
        }

        [Test]
        public void ItemUserDiscountString_FormatsAsPercentage()
        {
            var item = CreateItem(userDiscount: 0.10f);

            Assert.AreEqual("-10%", item.ItemUserDiscountString);
        }

        [Test]
        public void ItemDescription_ConcatenatesNameAndProducer()
        {
            var item = CreateItem(name: "Paracetamol", producer: "Pharma");

            Assert.AreEqual("Paracetamol - Pharma", item.ItemDescription);
        }

        [Test]
        public void ItemQuantityString_ReflectsCurrentQuantity()
        {
            var item = CreateItem(quantity: 5);

            Assert.AreEqual("Quantity: 5", item.ItemQuantityString);
        }

        [Test]
        public void SetFinalPrices_UpdatesFinalPriceBeforeDiscount()
        {
            var item = CreateItem();

            item.SetFinalPrices(50f, 40f);

            Assert.AreEqual(50f, item.FinalPriceBeforeDiscount, 0.001f);
        }

        [Test]
        public void SetFinalPrices_UpdatesFinalPriceAfterDiscount()
        {
            var item = CreateItem();

            item.SetFinalPrices(50f, 40f);

            Assert.AreEqual(40f, item.FinalPriceAfterDiscount, 0.001f);
        }

        [Test]
        public void ItemFinalPriceString_FormatsWithTwoDecimalsAndRON()
        {
            var item = CreateItem();
            item.SetFinalPrices(12.5f, 10f);

            Assert.AreEqual("12.50 RON", item.ItemFinalPriceString);
        }

        [Test]
        public void ItemFinalDiscountedPriceString_FormatsWithTwoDecimalsAndRON()
        {
            var item = CreateItem();
            item.SetFinalPrices(12.5f, 9.99f);

            Assert.AreEqual("9.99 RON", item.ItemFinalDiscountedPriceString);
        }

        [Test]
        public void ItemQuantityInBasket_SetBelowZero_ClampsToZero()
        {
            var item = CreateItem(quantity: 3);

            item.ItemQuantityInBasket = -1;

            Assert.AreEqual(0, item.ItemQuantityInBasket);
        }

        [Test]
        public void ItemQuantityInBasket_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            var item = CreateItem(quantity: 3);
            int changeCount = 0;
            item.PropertyChanged += (_, _) => changeCount++;

            item.ItemQuantityInBasket = 3;

            Assert.AreEqual(0, changeCount);
        }

        [Test]
        public void ItemQuantityInBasket_SetToNewValue_RaisesPropertyChanged()
        {
            var item = CreateItem(quantity: 3);
            bool raised = false;
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BasketItemViewModel.ItemQuantityInBasket))
                    raised = true;
            };

            item.ItemQuantityInBasket = 5;

            Assert.IsTrue(raised);
        }

        [Test]
        public void ItemQuantityInBasket_SetToNewValue_AlsoRaisesQuantityStringChanged()
        {
            var item = CreateItem(quantity: 3);
            bool raised = false;
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BasketItemViewModel.ItemQuantityString))
                    raised = true;
            };

            item.ItemQuantityInBasket = 5;

            Assert.IsTrue(raised);
        }

        [Test]
        public void SetFinalPrices_SameValue_DoesNotRaisePropertyChanged()
        {
            var item = CreateItem();
            item.SetFinalPrices(10f, 8f);
            int changeCount = 0;
            item.PropertyChanged += (_, _) => changeCount++;

            item.SetFinalPrices(10f, 8f);

            Assert.AreEqual(0, changeCount);
        }

        [Test]
        public void Equals_SameItemId_ReturnsTrue()
        {
            var a = CreateItem(id: 5);
            var b = CreateItem(id: 5);

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentItemId_ReturnsFalse()
        {
            var a = CreateItem(id: 1);
            var b = CreateItem(id: 2);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var a = CreateItem(id: 1);

            Assert.IsFalse(a.Equals((BasketItemViewModel)null));
        }

        [Test]
        public void Equals_ObjectOverload_SameId_ReturnsTrue()
        {
            var a = CreateItem(id: 5);
            var b = CreateItem(id: 5);

            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GetHashCode_SameId_ReturnsSameHash()
        {
            var a = CreateItem(id: 7);
            var b = CreateItem(id: 7);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }

    [TestFixture]
    public class BasketViewModelTests
    {
        private Mock<IOrderService> mockOrderService;

        [SetUp]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(s => s.GetBasketItems())
                .Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(0f, 0f));
        }

        private BasketItemViewModel MakeItem(int id = 1, int quantity = 2, float price = 10f)
        {
            var item = new BasketItemViewModel(id, "img", "Med", "Prod", quantity, 0f, 0f, 0f, price);
            item.SetFinalPrices(quantity * price, quantity * price);
            return item;
        }

        [Test]
        public void Constructor_LoadsBasketItemsFromService()
        {
            var items = new List<BasketItemViewModel> { MakeItem(1) };
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(items);

            var vm = new BasketViewModel(mockOrderService.Object);

            Assert.AreEqual(1, vm.BasketItems.Count);
        }

        [Test]
        public void Constructor_SetsTotalPriceString()
        {
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(30f, 25f));

            var vm = new BasketViewModel(mockOrderService.Object);

            Assert.AreEqual("30.00 RON", vm.TotalPriceString);
        }

        [Test]
        public void Constructor_SetsTotalDiscountedPriceString()
        {
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(30f, 25f));

            var vm = new BasketViewModel(mockOrderService.Object);

            Assert.AreEqual("25.00 RON", vm.TotalDiscountedPriceString);
        }

        [Test]
        public void RemoveItemCommand_ValidItem_RemovesFromBasketItems()
        {
            var item = MakeItem(1);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            vm.RemoveItemCommand.Execute(item);

            Assert.AreEqual(0, vm.BasketItems.Count);
        }

        [Test]
        public void RemoveItemCommand_ValidItem_CallsRemoveFromBasket()
        {
            var item = MakeItem(1);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            vm.RemoveItemCommand.Execute(item);

            mockOrderService.Verify(s => s.RemoveFromBasket(1), Times.Once);
        }

        [Test]
        public void RemoveItemCommand_NullItem_DoesNotThrow()
        {
            var vm = new BasketViewModel(mockOrderService.Object);

            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(null));
        }

        [Test]
        public void RemoveItemCommand_ValidItem_RaisesBasketQuantityRemoved()
        {
            var item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);
            int? capturedQuantity = null;
            vm.BasketQuantityRemoved += q => capturedQuantity = q;

            vm.RemoveItemCommand.Execute(item);

            Assert.IsNotNull(capturedQuantity);
        }

        [Test]
        public void RemoveItemCommand_AfterRemoval_RaisesEventWithRemainingQuantity()
        {
            var item1 = MakeItem(1, quantity: 2);
            var item2 = MakeItem(2, quantity: 3);
            mockOrderService.Setup(s => s.GetBasketItems())
                .Returns(new List<BasketItemViewModel> { item1, item2 });
            var vm = new BasketViewModel(mockOrderService.Object);
            int? capturedQuantity = null;
            vm.BasketQuantityRemoved += q => capturedQuantity = q;

            vm.RemoveItemCommand.Execute(item1);

            Assert.AreEqual(3, capturedQuantity);
        }

        [Test]
        public void TotalPriceString_SetNewValue_RaisesPropertyChanged()
        {
            var vm = new BasketViewModel(mockOrderService.Object);
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BasketViewModel.TotalPriceString))
                    raised = true;
            };

            vm.TotalPriceString = "99.00 RON";

            Assert.IsTrue(raised);
        }

        [Test]
        public void TotalPriceString_SetSameValue_DoesNotRaisePropertyChanged()
        {
            var vm = new BasketViewModel(mockOrderService.Object);
            vm.TotalPriceString = "5.00 RON";
            int count = 0;
            vm.PropertyChanged += (_, _) => count++;

            vm.TotalPriceString = "5.00 RON";

            Assert.AreEqual(0, count);
        }

        [Test]
        public void TotalDiscountedPriceString_SetNewValue_RaisesPropertyChanged()
        {
            var vm = new BasketViewModel(mockOrderService.Object);
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BasketViewModel.TotalDiscountedPriceString))
                    raised = true;
            };

            vm.TotalDiscountedPriceString = "80.00 RON";

            Assert.IsTrue(raised);
        }

        [Test]
        public void GetPrescription_ValidId_ReloadsBasketItems()
        {
            var vm = new BasketViewModel(mockOrderService.Object);
            var newItems = new List<BasketItemViewModel> { MakeItem(5) };
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(newItems);

            vm.GetPrescription("RX001");

            Assert.AreEqual(1, vm.BasketItems.Count);
        }

        [Test]
        public void GetPrescription_ValidId_CallsApplyPrescriptionOnService()
        {
            var vm = new BasketViewModel(mockOrderService.Object);

            vm.GetPrescription("RX001");

            mockOrderService.Verify(s => s.ApplyPrescriptionToBasket("RX001"), Times.Once);
        }

        [Test]
        public void GetPrescription_ValidId_RaisesBasketQuantityRemoved()
        {
            var vm = new BasketViewModel(mockOrderService.Object);
            bool raised = false;
            vm.BasketQuantityRemoved += _ => raised = true;

            vm.GetPrescription("RX001");

            Assert.IsTrue(raised);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityReachesZero_RemovesItemFromBasketItems()
        {
            var item = MakeItem(1, quantity: 1);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 0;

            Assert.AreEqual(0, vm.BasketItems.Count);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityReachesZero_CallsRemoveFromBasket()
        {
            var item = MakeItem(1, quantity: 1);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 0;

            mockOrderService.Verify(s => s.RemoveFromBasket(1), Times.Once);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityUpdated_CallsUpdateBasketItemQuantity()
        {
            var item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 5;

            mockOrderService.Verify(s => s.UpdateBasketItemQuantity(1, 5), Times.Once);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityUpdated_CallsRecalculateBasketItemPrices()
        {
            var item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            var vm = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 5;

            mockOrderService.Verify(s => s.RecalculateBasketItemPrices(item), Times.AtLeastOnce);
        }
    }
}