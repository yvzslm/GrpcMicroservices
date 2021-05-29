using ShoppingCartGrpc.Models;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeeder
    {
        public static void Seed(ShoppingCartContext context)
        {
            if (!context.ShoppingCart.Any())
            {
                var shoppingCarts = new List<ShoppingCart>()
                {
                    new ShoppingCart()
                    {
                        UserName = "swn",
                        Items = new List<ShoppingCartItem>()
                        {
                            new ShoppingCartItem()
                            {
                                Quantity = 2,
                                Color = "Black",
                                Price = 699,
                                ProductId = 1,
                                ProductName = "Mi10T"
                            },
                             new ShoppingCartItem()
                            {
                                Quantity = 3,
                                Color = "Red",
                                Price = 899,
                                ProductId = 2,
                                ProductName = "P40"
                            },
                        }
                    }
                };

                context.ShoppingCart.AddRange(shoppingCarts);
                context.SaveChanges();
            }
        }
    }
}
