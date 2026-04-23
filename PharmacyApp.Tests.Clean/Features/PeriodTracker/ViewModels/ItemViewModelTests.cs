using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Features.Period_Tracker.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.ViewModels
{
    [TestFixture]
    public class ItemViewModelTests
    {
        private Mock<IBasketService> basketServiceMock = null!;

        [SetUp]
        public void SetUp()
        {
            basketServiceMock = new Mock<IBasketService>();
        }

        [Test]
        public void Constructor_WhenDiscountsExist_SetsDiscountedPricePresentation()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: string.Empty,
                description: string.Empty,
                imagePath: "..\\Assets\\tea.png",
                discount: 10f);

            ItemViewModel viewModel = new ItemViewModel(item, 20f, basketServiceMock.Object);

            float expectedFinalPrice = 100f * (1f - 10f / 100f) * (1f - 20f / 100f);

            Assert.That(
                MatchesDiscountedPresentation(
                    viewModel,
                    100f.ToString("C", CultureInfo.CurrentCulture),
                    expectedFinalPrice.ToString("C", CultureInfo.CurrentCulture),
                    "Gray",
                    "Green"),
                Is.True);
        }

        [Test]
        public void Constructor_WhenNoDiscountExists_SetsRegularPricePresentation()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "..\\..\\Assets\\placeholder.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(
                MatchesRegularPresentation(
                    viewModel,
                    100f.ToString("C", CultureInfo.CurrentCulture)),
                Is.True);
        }

        [Test]
        public void Constructor_WhenImagePathIsNull_UsesPlaceholderPath()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: null!,
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(viewModel.ImagePath, Is.EqualTo("ms-appx:///Assets/placeholder.png"));
        }

        [Test]
        public void Constructor_WhenImagePathAlreadyUsesApplicationPrefix_KeepsOriginalValue()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "ms-appx:///Assets/test.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(viewModel.ImagePath, Is.EqualTo("ms-appx:///Assets/test.png"));
        }

        [Test]
        public void Constructor_WhenImagePathUsesWindowsSlashes_ConvertsPathToMsAppxFormat()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "..\\..\\Assets\\test.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(viewModel.ImagePath, Is.EqualTo("ms-appx:///../Assets/test.png"));
        }

        [Test]
        public void AddToBasketCommand_WhenExecuted_AddsCurrentItemWithDefaultQuantityAndDiscount()
        {
            Item item = new Item(
                id: 9,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 20f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "..\\..\\Assets\\placeholder.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 15f, basketServiceMock.Object);

            viewModel.AddToBasketCommand.Execute(null);

            basketServiceMock.Verify(service => service.AddToBasket(9, 1, 15f), Times.Once);
        }

        [Test]
        public void Constructor_WhenItemHasOnlyOwnDiscount_ComputesDiscountedPriceCorrectly()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "Assets/test.png",
                discount: 10f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(
                MatchesDiscountedPresentation(
                    viewModel,
                    100f.ToString("C", CultureInfo.CurrentCulture),
                    90f.ToString("C", CultureInfo.CurrentCulture),
                    "Gray",
                    "Green"),
                Is.True);
        }

        [Test]
        public void Constructor_WhenItemHasOnlyExtraDiscount_ComputesDiscountedPriceCorrectly()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "Assets/test.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 20f, basketServiceMock.Object);

            Assert.That(
                MatchesDiscountedPresentation(
                    viewModel,
                    100f.ToString("C", CultureInfo.CurrentCulture),
                    80f.ToString("C", CultureInfo.CurrentCulture),
                    "Gray",
                    "Green"),
                Is.True);
        }

        [Test]
        public void Constructor_WhenItemHasOwnAndExtraDiscount_AppliesBothDiscountsSequentially()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "Assets/test.png",
                discount: 10f);

            ItemViewModel viewModel = new ItemViewModel(item, 20f, basketServiceMock.Object);

            Assert.That(
                MatchesPriceStrings(
                    viewModel,
                    72f.ToString("C", CultureInfo.CurrentCulture),
                    100f.ToString("C", CultureInfo.CurrentCulture)),
                Is.True);
        }

        [Test]
        public void Constructor_WhenImagePathHasNoPrefixAndNoLeadingSlash_AddsApplicationPrefixAndLeadingSlash()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "Assets/test.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            Assert.That(viewModel.ImagePath, Is.EqualTo("ms-appx:///Assets/test.png"));
        }

        [Test]
        public void PropertySetters_WhenSetToSameValue_DoNotRaisePropertyChanged()
        {
            Item item = new Item(
                id: 1,
                name: "Tea",
                producer: "Producer",
                category: "wellness",
                price: 100f,
                numberOfPills: 1,
                label: "",
                description: "",
                imagePath: "Assets/test.png",
                discount: 0f);

            ItemViewModel viewModel = new ItemViewModel(item, 0f, basketServiceMock.Object);

            int propertyChangedCalls = 0;
            viewModel.PropertyChanged += (_, _) => propertyChangedCalls++;

            viewModel.Name = viewModel.Name;
            viewModel.PriceString = viewModel.PriceString;
            viewModel.PriceDiscountedString = viewModel.PriceDiscountedString;
            viewModel.PriceColor = viewModel.PriceColor;
            viewModel.FinalPriceColor = viewModel.FinalPriceColor;
            viewModel.ImagePath = viewModel.ImagePath;

            Assert.That(propertyChangedCalls, Is.EqualTo(0));
        }

        private static bool MatchesDiscountedPresentation(
            ItemViewModel viewModel,
            string expectedOriginalPrice,
            string expectedFinalPrice,
            string expectedPriceColor,
            string expectedFinalPriceColor)
        {
            return viewModel.PriceDiscountedString == expectedOriginalPrice
                && viewModel.PriceString == expectedFinalPrice
                && viewModel.PriceColor == expectedPriceColor
                && viewModel.FinalPriceColor == expectedFinalPriceColor;
        }

        private static bool MatchesRegularPresentation(ItemViewModel viewModel, string expectedPrice)
        {
            return viewModel.PriceDiscountedString == string.Empty
                && viewModel.PriceColor == "Transparent"
                && viewModel.FinalPriceColor == "Black"
                && viewModel.PriceString == expectedPrice;
        }

        private static bool MatchesPriceStrings(
            ItemViewModel viewModel,
            string expectedFinalPrice,
            string expectedOriginalPrice)
        {
            return viewModel.PriceString == expectedFinalPrice
                && viewModel.PriceDiscountedString == expectedOriginalPrice;
        }
    }
}