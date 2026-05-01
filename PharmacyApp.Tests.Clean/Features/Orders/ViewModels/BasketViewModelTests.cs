using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;

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
            return new BasketItemViewModel(
                id,
                imagePath,
                name,
                producer,
                quantity,
                baseDiscount,
                extraDiscount,
                userDiscount,
                price);
        }

        [Test]
        public void Constructor_NegativeQuantity_ClampsToZero()
        {
            BasketItemViewModel item = CreateItem(quantity: -5);

            Assert.That(item.ItemQuantityInBasket, Is.EqualTo(0));
        }

        [Test]
        public void ItemActiveDiscount_WithBaseAndExtra_CombinesCorrectly()
        {
            BasketItemViewModel item = CreateItem(baseDiscount: 0.1f, extraDiscount: 0.2f);

            Assert.That(item.ItemActiveDiscount, Is.EqualTo(0.28f).Within(0.001f));
        }

        [Test]
        public void ItemActiveDiscount_WithZeroDiscounts_IsZero()
        {
            BasketItemViewModel item = CreateItem(baseDiscount: 0f, extraDiscount: 0f);

            Assert.That(item.ItemActiveDiscount, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void ItemDiscountString_FormatsAsPercentage()
        {
            BasketItemViewModel item = CreateItem(baseDiscount: 0.25f, extraDiscount: 0f);

            Assert.That(item.ItemDiscountString, Is.EqualTo("-25%"));
        }

        [Test]
        public void ItemUserDiscountString_FormatsAsPercentage()
        {
            BasketItemViewModel item = CreateItem(userDiscount: 0.10f);

            Assert.That(item.ItemUserDiscountString, Is.EqualTo("-10%"));
        }

        [Test]
        public void ItemDescription_ConcatenatesNameAndProducer()
        {
            BasketItemViewModel item = CreateItem(name: "Paracetamol", producer: "Pharma");

            Assert.That(item.ItemDescription, Is.EqualTo("Paracetamol - Pharma"));
        }

        [Test]
        public void ItemQuantityString_ReflectsCurrentQuantity()
        {
            BasketItemViewModel item = CreateItem(quantity: 5);

            Assert.That(item.ItemQuantityString, Is.EqualTo("Quantity: 5"));
        }

        [Test]
        public void SetFinalPrices_UpdatesFinalPriceBeforeDiscount()
        {
            BasketItemViewModel item = CreateItem();

            item.SetFinalPrices(50f, 40f);

            Assert.That(item.FinalPriceBeforeDiscount, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void SetFinalPrices_UpdatesFinalPriceAfterDiscount()
        {
            BasketItemViewModel item = CreateItem();

            item.SetFinalPrices(50f, 40f);

            Assert.That(item.FinalPriceAfterDiscount, Is.EqualTo(40f).Within(0.001f));
        }

        [Test]
        public void ItemFinalPriceString_FormatsWithTwoDecimalsAndRON()
        {
            BasketItemViewModel item = CreateItem();
            item.SetFinalPrices(12.5f, 10f);

            Assert.That(item.ItemFinalPriceString, Is.EqualTo("12,50 RON"));
        }

        [Test]
        public void ItemFinalDiscountedPriceString_FormatsWithTwoDecimalsAndRON()
        {
            BasketItemViewModel item = CreateItem();
            item.SetFinalPrices(12.5f, 9.99f);

            Assert.That(item.ItemFinalDiscountedPriceString, Is.EqualTo("9,99 RON"));
        }

        [Test]
        public void ItemQuantityInBasket_SetBelowZero_ClampsToZero()
        {
            BasketItemViewModel item = CreateItem(quantity: 3);

            item.ItemQuantityInBasket = -1;

            Assert.That(item.ItemQuantityInBasket, Is.EqualTo(0));
        }

        [Test]
        public void ItemQuantityInBasket_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            BasketItemViewModel item = CreateItem(quantity: 3);
            int changeCount = 0;
            item.PropertyChanged += (_, _) => changeCount++;

            item.ItemQuantityInBasket = 3;

            Assert.That(changeCount, Is.EqualTo(0));
        }

        [Test]
        public void ItemQuantityInBasket_SetToNewValue_RaisesPropertyChanged()
        {
            BasketItemViewModel item = CreateItem(quantity: 3);
            bool raised = false;
            item.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BasketItemViewModel.ItemQuantityInBasket))
                {
                    raised = true;
                }
            };

            item.ItemQuantityInBasket = 5;

            Assert.That(raised, Is.True);
        }

        [Test]
        public void ItemQuantityInBasket_SetToNewValue_AlsoRaisesQuantityStringChanged()
        {
            BasketItemViewModel item = CreateItem(quantity: 3);
            bool raised = false;
            item.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BasketItemViewModel.ItemQuantityString))
                {
                    raised = true;
                }
            };

            item.ItemQuantityInBasket = 5;

            Assert.That(raised, Is.True);
        }

        [Test]
        public void SetFinalPrices_SameValue_DoesNotRaisePropertyChanged()
        {
            BasketItemViewModel item = CreateItem();
            item.SetFinalPrices(10f, 8f);
            int changeCount = 0;
            item.PropertyChanged += (_, _) => changeCount++;

            item.SetFinalPrices(10f, 8f);

            Assert.That(changeCount, Is.EqualTo(0));
        }

        [Test]
        public void Equals_SameItemId_ReturnsTrue()
        {
            BasketItemViewModel firstItem = CreateItem(id: 5);
            BasketItemViewModel secondItem = CreateItem(id: 5);

            Assert.That(firstItem.Equals(secondItem), Is.True);
        }

        [Test]
        public void Equals_DifferentItemId_ReturnsFalse()
        {
            BasketItemViewModel firstItem = CreateItem(id: 1);
            BasketItemViewModel secondItem = CreateItem(id: 2);

            Assert.That(firstItem.Equals(secondItem), Is.False);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            BasketItemViewModel item = CreateItem(id: 1);

            Assert.That(item.Equals((BasketItemViewModel)null), Is.False);
        }

        [Test]
        public void Equals_ObjectOverload_SameId_ReturnsTrue()
        {
            BasketItemViewModel firstItem = CreateItem(id: 5);
            BasketItemViewModel secondItem = CreateItem(id: 5);

            Assert.That(firstItem.Equals((object)secondItem), Is.True);
        }

        [Test]
        public void GetHashCode_SameId_ReturnsSameHash()
        {
            BasketItemViewModel firstItem = CreateItem(id: 7);
            BasketItemViewModel secondItem = CreateItem(id: 7);

            Assert.That(firstItem.GetHashCode(), Is.EqualTo(secondItem.GetHashCode()));
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
                .Setup(service => service.GetBasketItems())
                .Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(0f, 0f));
        }

        private BasketItemViewModel MakeItem(int id = 1, int quantity = 2, float price = 10f)
        {
            BasketItemViewModel item = new BasketItemViewModel(id, "img", "Med", "Prod", quantity, 0f, 0f, 0f, price);
            item.SetFinalPrices(quantity * price, quantity * price);
            return item;
        }

        [Test]
        public void Constructor_LoadsBasketItemsFromService()
        {
            List<BasketItemViewModel> items = new List<BasketItemViewModel> { MakeItem(1) };
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(items);

            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            Assert.That(viewModel.BasketItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_SetsTotalPriceString()
        {
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(30f, 25f));

            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            Assert.That(viewModel.TotalPriceString, Is.EqualTo("30,00 RON"));
        }

        [Test]
        public void Constructor_SetsTotalDiscountedPriceString()
        {
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(30f, 25f));

            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            Assert.That(viewModel.TotalDiscountedPriceString, Is.EqualTo("25,00 RON"));
        }

        [Test]
        public void RemoveItemCommand_ValidItem_RemovesFromBasketItems()
        {
            BasketItemViewModel item = MakeItem(1);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            viewModel.RemoveItemCommand.Execute(item);

            Assert.That(viewModel.BasketItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveItemCommand_ValidItem_CallsRemoveFromBasket()
        {
            BasketItemViewModel item = MakeItem(1);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            viewModel.RemoveItemCommand.Execute(item);

            mockOrderService.Verify(service => service.RemoveFromBasket(1), Times.Once);
        }

        [Test]
        public void RemoveItemCommand_NullItem_DoesNotThrow()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            Assert.That(() => viewModel.RemoveItemCommand.Execute(null), Throws.Nothing);
        }

        [Test]
        public void RemoveItemCommand_ValidItem_RaisesBasketQuantityRemoved()
        {
            BasketItemViewModel item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            int? capturedQuantity = null;
            viewModel.BasketQuantityRemoved += quantity => capturedQuantity = quantity;

            viewModel.RemoveItemCommand.Execute(item);

            Assert.That(capturedQuantity, Is.Not.Null);
        }

        [Test]
        public void RemoveItemCommand_AfterRemoval_RaisesEventWithRemainingQuantity()
        {
            BasketItemViewModel firstItem = MakeItem(1, quantity: 2);
            BasketItemViewModel secondItem = MakeItem(2, quantity: 3);

            mockOrderService
                .Setup(service => service.GetBasketItems())
                .Returns(new List<BasketItemViewModel> { firstItem, secondItem });

            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            int? capturedQuantity = null;
            viewModel.BasketQuantityRemoved += quantity => capturedQuantity = quantity;

            viewModel.RemoveItemCommand.Execute(firstItem);

            Assert.That(capturedQuantity, Is.EqualTo(3));
        }

        [Test]
        public void TotalPriceString_SetNewValue_RaisesPropertyChanged()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            bool raised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BasketViewModel.TotalPriceString))
                {
                    raised = true;
                }
            };

            viewModel.TotalPriceString = "99.00 RON";

            Assert.That(raised, Is.True);
        }

        [Test]
        public void TotalPriceString_SetSameValue_DoesNotRaisePropertyChanged()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            viewModel.TotalPriceString = "5.00 RON";
            int count = 0;
            viewModel.PropertyChanged += (_, _) => count++;

            viewModel.TotalPriceString = "5.00 RON";

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void TotalDiscountedPriceString_SetNewValue_RaisesPropertyChanged()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            bool raised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BasketViewModel.TotalDiscountedPriceString))
                {
                    raised = true;
                }
            };

            viewModel.TotalDiscountedPriceString = "80.00 RON";

            Assert.That(raised, Is.True);
        }

        [Test]
        public void GetPrescription_ValidId_ReloadsBasketItems()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            List<BasketItemViewModel> newItems = new List<BasketItemViewModel> { MakeItem(5) };
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(newItems);

            viewModel.GetPrescription("RX001");

            Assert.That(viewModel.BasketItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPrescription_ValidId_CallsApplyPrescriptionOnService()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            viewModel.GetPrescription("RX001");

            mockOrderService.Verify(service => service.ApplyPrescriptionToBasket("RX001"), Times.Once);
        }

        [Test]
        public void GetPrescription_ValidId_RaisesBasketQuantityRemoved()
        {
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);
            bool raised = false;
            viewModel.BasketQuantityRemoved += _ => raised = true;

            viewModel.GetPrescription("RX001");

            Assert.That(raised, Is.True);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityReachesZero_RemovesItemFromBasketItems()
        {
            BasketItemViewModel item = MakeItem(1, quantity: 1);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 0;

            Assert.That(viewModel.BasketItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityReachesZero_CallsRemoveFromBasket()
        {
            BasketItemViewModel item = MakeItem(1, quantity: 1);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 0;

            mockOrderService.Verify(service => service.RemoveFromBasket(1), Times.Once);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityUpdated_CallsUpdateBasketItemQuantity()
        {
            BasketItemViewModel item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 5;

            mockOrderService.Verify(service => service.UpdateBasketItemQuantity(1, 5), Times.Once);
        }

        [Test]
        public void QuantityChanged_WhenItemQuantityUpdated_CallsRecalculateBasketItemPrices()
        {
            BasketItemViewModel item = MakeItem(1, quantity: 2);
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel> { item });
            BasketViewModel viewModel = new BasketViewModel(mockOrderService.Object);

            item.ItemQuantityInBasket = 5;

            mockOrderService.Verify(service => service.RecalculateBasketItemPrices(item), Times.AtLeastOnce);
        }
    }
}