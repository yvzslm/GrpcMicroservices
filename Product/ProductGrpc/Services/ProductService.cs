using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductContext _productContext;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(ProductContext productContext, ILogger<ProductService> logger, IMapper mapper)
        {
            _productContext = productContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID: {request.ProductId} was not found."));
            }

            return _mapper.Map<ProductModel>(product);
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Product.ToListAsync();
            foreach (var product in productList)
            {
                await responseStream.WriteAsync(_mapper.Map<ProductModel>(product));
                await Task.Delay(1000);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);
            await _productContext.Product.AddAsync(product);
            await _productContext.SaveChangesAsync();

            _logger.LogInformation($"Product added succesfully. Product Id: {product.ProductId}");

            return _mapper.Map<ProductModel>(product);
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);
            bool isExist = await _productContext.Product.AnyAsync(p => p.ProductId == product.ProductId);
            if (!isExist)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID: {request.Product.ProductId} was not found."));
            }

            _productContext.Entry(product).State = EntityState.Modified;
            await _productContext.SaveChangesAsync();

            return _mapper.Map<ProductModel>(product);
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID: {request.ProductId} was not found."));
            }

            _productContext.Product.Remove(product);
            var deleteCount = await _productContext.SaveChangesAsync();

            return new DeleteProductResponse() { Success = deleteCount > 0 };
        }

        public override async Task<InsertBulkProductResponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                await _productContext.Product.AddAsync(_mapper.Map<Product>(requestStream.Current));
            }

            var insertCount = await _productContext.SaveChangesAsync();

            return new InsertBulkProductResponse() { InsertCount = insertCount, Success = insertCount > 0 };
        }
    }
}
