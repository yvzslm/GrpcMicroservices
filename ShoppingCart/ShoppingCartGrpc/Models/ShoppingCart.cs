using System.Collections.Generic;

namespace ShoppingCartGrpc.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public ShoppingCart()
        {

        }

        public ShoppingCart(string userName)
        {
            UserName = userName;
        }

        public float TotalPrice
        {
            get
            {
                float totalPrice = 0;
                Items.ForEach(i =>
                {
                    totalPrice += i.Price * i.Quantity;
                });

                return totalPrice;
            }
        }
    }
}
