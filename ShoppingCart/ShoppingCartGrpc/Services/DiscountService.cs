using DiscountGrpc.Protos;
using System.Threading.Tasks;
using static DiscountGrpc.Protos.DiscountProtoService;

namespace ShoppingCartGrpc.Services
{
    public class DiscountService
    {
        private readonly DiscountProtoServiceClient _client;

        public DiscountService(DiscountProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<DiscountModel> GetDiscountAsync(string discountCode)
        {
            var request = new GetDiscountRequest() { DiscountCode = discountCode };
            return await _client.GetDiscountAsync(request);
        }
    }
}
