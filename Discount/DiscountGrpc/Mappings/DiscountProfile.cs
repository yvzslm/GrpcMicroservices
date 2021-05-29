using AutoMapper;
using DiscountGrpc.Models;
using DiscountGrpc.Protos;

namespace DiscountGrpc.Mappings
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<Discount, DiscountModel>().ReverseMap();
        }
    }
}
