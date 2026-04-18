using System;
using System.Collections.Generic;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Orders.Logic
{
    public interface IOrderService
    {
        ISubstancesRepository SubstancesRepository { get; }
        IItemsRepository ItemsRepository { get; }
        IUsersRepository UsersRepository { get; }
        IOrdersRepository OrdersRepository { get; }
        User ActiveUser { get; }

        void AddToBasket(int itemId, int quantityToBuy);
        void AddToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f);
        void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy);
        void RemoveFromBasket(int itemIdToRemove);
        void PlaceOrderFromBasket(DateOnly chosenPickUpDate);
        void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities);
        void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, Tuple<int, float>> updatedQuantities, DateOnly updatedPickUpDate);
        void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate);
        void CancelOrder(int orderId);
        Dictionary<int, int> FillBasketFromPrescription(string prescriptionId);
    }
}
