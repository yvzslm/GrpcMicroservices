using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System.Linq;
using System.Threading.Tasks;
using static ShoppingCartGrpc.Protos.ShoppingCartProtoService;

namespace ShoppingCartGrpc.Services
{
    [Authorize]
    public class ShoppingCartService : ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;
        private readonly DiscountService _discountService;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, 
                                   IMapper mapper, DiscountService discountService)
        {
            _shoppingCartContext = shoppingCartContext;
            _logger = logger;
            _mapper = mapper;
            _discountService = discountService;
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username: {request.Username} was not found."));
            }

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            var isExist = await _shoppingCartContext.ShoppingCart.AnyAsync(s => s.UserName == shoppingCart.UserName);
            if (isExist)
            {
                _logger.LogError($"Invalid username for ShoppingCart creation. Username: {shoppingCart.UserName} is already exist.");
                throw new RpcException(new Status(StatusCode.AlreadyExists, 
                                        $"ShoppingCart with username: {shoppingCart.UserName} is already exist."));
            }

            await _shoppingCartContext.ShoppingCart.AddAsync(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation($"ShoppingCart is successfully created. UserName: {shoppingCart.UserName}");

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, 
                                            $"ShoppingCart with username: {requestStream.Current.Username} was not found."));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);
                if (cartItem == null)
                {
                    var discount = await _discountService.GetDiscountAsync(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;
                    shoppingCart.Items.Add(newAddedCartItem);
                }
                else
                {
                    cartItem.Quantity++;
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();
            return new AddItemIntoShoppingCartResponse() { InsertCount = insertCount, Success = insertCount > 0 };
        }

        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCart(RemoveItemIntoShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username: {request.Username} was not found."));
            }

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.Id == request.NewCartItem.ProductId);
            if (removeCartItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                                        $"ShoppingCartItem with ProductId: {request.NewCartItem.ProductId} was not found in the ShoppingCart."));
            }

            shoppingCart.Items.Remove(removeCartItem);
            var removeCount = await _shoppingCartContext.SaveChangesAsync();

            return new RemoveItemIntoShoppingCartResponse() { Success = removeCount > 0 };
        }
    }
}
