using AutoMapper;
using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using static DiscountGrpc.Protos.DiscountProtoService;

namespace DiscountGrpc
{
    public class DiscountService : DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;
        private readonly IMapper _mapper;

        public DiscountService(ILogger<DiscountService> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(d => d.Code == request.DiscountCode);
            if (discount == null)
            {
                _logger.LogError($"Invalid discount code: {request.DiscountCode}");
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount with code: {request.DiscountCode} was not found."));
            }

            return Task.FromResult(_mapper.Map<DiscountModel>(discount));
        }
    }
}
