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

        // === Funcții adăugate de noi pentru Afișare ===
        List<Order> GetClientOrders(int clientId);
        List<Order> GetAllOrders();
        void UpdateOrdersExpirationStatus();

        // === Funcții Modificate (Am eliminat Tuple) ===
        void PlaceOrderFromBasket(DateOnly chosenPickUpDate);
        // Observă că acum primește Dictionary<int, OrderItem> în loc de Dictionary<int, Tuple<int, float>>
        void CompleteOrder(int orderId, Dictionary<int, OrderItem> updatedItems);
        void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, OrderItem> updatedItems, DateOnly updatedPickUpDate);
        void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate);
        void CancelOrder(int orderId);

        // === Funcțiile lui Paul de Basket (Lăsate INTacte) ===
        void AddToBasket(int itemId, int quantityToBuy);
        void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f);
        void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy);
        void RemoveFromBasket(int itemIdToRemove);
        Dictionary<int, int> FillBasketFromPrescription(string prescriptionId);
        List<BasketItemViewModel> GetBasketItems();
        void ApplyPrescriptionToBasket(string prescriptionId);
        void RecalculateBasketItemPrices(BasketItemViewModel basketItem);
        Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems);

        Order GetOrderById(int orderIdentifier);
        Item GetItemById(int itemIdentifier);
    }
}