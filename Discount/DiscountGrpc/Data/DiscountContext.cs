﻿using DiscountGrpc.Models;
using System.Collections.Generic;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discounts = new()
        {
            new Discount() { Id = 1, Code = "CODE_100", Amount = 100 },
            new Discount() { Id = 2, Code = "CODE_200", Amount = 200 },
            new Discount() { Id = 3, Code = "CODE_300", Amount = 300 },
        };
    }
}