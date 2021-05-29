using ProductGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductGrpc.Data
{
    public class ProductContextSeeder
    {
        public static void Seed(ProductContext context)
        {
            if (!context.Product.Any())
            {
                var products = new List<Product>()
                {
                    new Product()
                    {
                        ProductId = 1,
                        Name = "Mi 10T",
                        Description = "New Xiaomi Phone Mi 10T",
                        Price = 659,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product()
                    {
                        ProductId = 2,
                        Name = "P40",
                        Description = "New Huawei Phone P40",
                        Price = 899,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product()
                    {
                        ProductId = 3,
                        Name = "A50",
                        Description = "New Samsung Phone A50",
                        Price = 399,
                        Status = ProductStatus.LOW,
                        CreatedTime = DateTime.UtcNow
                    }
                };

                context.Product.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
