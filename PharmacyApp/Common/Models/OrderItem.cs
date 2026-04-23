namespace PharmacyApp.Models
{
    public class OrderItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public float FinalPrice { get; set; }

        public OrderItem(int itemId, int quantity, float finalPrice)
        {
            ItemId = itemId;
            Quantity = quantity;
            FinalPrice = finalPrice;
        }
    }
}