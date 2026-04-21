using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Features.Orders.Logic
{
    public interface IOrderService
    {


        ISubstancesRepository SubstancesRepository { get; }
        IItemsRepository ItemsRepository { get; }
        IUsersRepository UsersRepository { get; }
        IOrdersRepository OrdersRepository { get; }
        User ActiveUser { get; }

        void PlaceOrderFromBasket(DateOnly chosenPickUpDate);
        void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities);
        void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, Tuple<int, float>> updatedQuantities, DateOnly updatedPickUpDate);
        void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate);
        void CancelOrder(int orderId);

        void AddToBasket(int itemId, int quantityToBuy);
        void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f);
        void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy);
        void RemoveFromBasket(int itemIdToRemove);
        Dictionary<int, int> FillBasketFromPrescription(string prescriptionId);
        List<BasketItemViewModel> GetBasketItems();
        void ApplyPrescriptionToBasket(string prescriptionId);
        void RecalculateBasketItemPrices(BasketItemViewModel basketItem);
        Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems);
    }
}
