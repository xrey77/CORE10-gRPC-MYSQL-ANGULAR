// Services/ProductService.cs
using System.Data;
using Dapper;
using Grpc.Core;

namespace core10_grpc_mysql.Services;

public class ProductService(ILogger<ProductService> logger, IDbConnection dbConnection) : Product.ProductBase
{
    public override async Task<ProductListResponse> ProductList(ProductListRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Descritions, Qty, Unit, Costprice, Sellprice  FROM products";

        var dbProducts = await dbConnection.QueryAsync<dynamic>(sql);

        if (dbProducts == null)
        {
            logger.LogError("Products not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found."));            
        }

        var response = new ProductListResponse();

        if (dbProducts != null)
        {
            var productList = new List<ProductData>();

            foreach (var product in dbProducts)
            {
                productList.Add(new ProductData
                {
                    Id = product.Id,
                    Descriptions = product.Descriptions,
                    Qty = product.Qty,
                    Unit = product.Unit,
                    Costprice = product.Costprice ?? 0,
                    Sellprice  = product.Sellprice ?? 0
                });
            }

            response.Data.AddRange(productList);
        }

        return response;
        
    }


    public override async Task<ProductSearchResponse> ProductSearch(ProductSearchRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Descritions, Qty, Unit, Costprice, Sellprice  FROM products";

        var dbProducts = await dbConnection.QueryAsync<dynamic>(sql);
        var searchKey = request.Keyword;

        if (dbProducts == null)
        {
            logger.LogError("Products not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "Product now found."));            
        }


        var response = new ProductSearchResponse();

        if (dbProducts != null)
        {
            // Create a temporary list to hold our mapped records
            var productList = new List<ProductData>();

            foreach (var product in dbProducts)
            {
                productList.Add(new ProductData
                {
                    Id = product.Id,
                    Descriptions = product.Descriptions,
                    Qty = product.Qty,
                    Unit = product.Unit,
                    Costprice = product.Costprice,
                    Sellprice  = product.Sellprice
                });
            }

            response.Data.AddRange(productList);
        }

        return response;

    }
}
