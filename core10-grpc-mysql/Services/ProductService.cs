// Services/ProductService.cs
using System.Data;
using Dapper;
using Grpc.Core;
using Google.Protobuf.Collections; 

namespace core10_grpc_mysql.Services;

public class ProductService(ILogger<ProductService> logger, IDbConnection dbConnection) : Product.ProductBase
{
    public override async Task<ProductListResponse> ProductList(ProductListRequest request, ServerCallContext context)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var perPage = 5;
        var offset = (page - 1) * perPage;

        const string countSql = "SELECT COUNT(*) FROM products";
        const string selectSql = @"
            SELECT Id, Descriptions, Qty, Unit, Costprice, Sellprice 
            FROM products 
            ORDER BY Id 
            LIMIT @Limit OFFSET @Offset";

        var totalRecords = await dbConnection.ExecuteScalarAsync<int>(countSql);
        
        var dbProducts = await dbConnection.QueryAsync<dynamic>(selectSql, new 
        { 
            Limit = perPage, 
            Offset = offset 
        });

        if (dbProducts == null)
        {
            logger.LogError("Products not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "Products not found."));
        }

        int totalPage = (int)Math.Ceiling((float)totalRecords / perPage);

        var response = new ProductListResponse
        {
            Page = page,
            TotalPage = totalPage,
            TotalRecords = totalRecords
        };

        var productList = dbProducts.Select(product => new ProductData
        {
            Id = product.Id,
            Descriptions = product.Descriptions ?? string.Empty,
            Qty = product.Qty ?? 0,
            Unit = product.Unit ?? string.Empty,
            Costprice = (double)product.Costprice,
            Sellprice = (double)product.Sellprice
        }).ToList();

        response.Data.AddRange(productList); 

        return response;
    }


    public override async Task<ProductSearchResponse> ProductSearch(ProductSearchRequest request, ServerCallContext context)
    {
        var searchTerm = $"%{request.Keyword}%";
        var page = request.Page <= 0 ? 1 : request.Page;
        var perPage = 5;
        var offset = (page - 1) * perPage;

        const string countSql = "SELECT COUNT(*) FROM products WHERE Descriptions LIKE @SearchTerm";
        
        const string selectSql = @"
            SELECT Id, Descriptions, Qty, Unit, Costprice, Sellprice 
            FROM products 
            WHERE Descriptions LIKE @SearchTerm
            ORDER BY Id 
            LIMIT @Limit OFFSET @Offset";

        var totalRecords = await dbConnection.ExecuteScalarAsync<int>(countSql, new
        {
            SearchTerm = searchTerm
        });

        var totalPage = (int)Math.Ceiling(totalRecords / (double)perPage);

        var dbProducts = await dbConnection.QueryAsync<ProductData>(selectSql, new 
        { 
            SearchTerm = searchTerm,
            Limit = perPage, 
            Offset = offset
        });

        var response = new ProductSearchResponse
        {
            Page = page,
            TotalPage = totalPage,
            TotalRecords = totalRecords
        };


        var productList = dbProducts.Select(product => new ProductData
        {
            Id = (int)product.Id,
            Descriptions = product.Descriptions ?? string.Empty,
            Qty = (int)product.Qty,
            Unit = product.Unit ?? string.Empty,
            Costprice = (double)product.Costprice,
            Sellprice = (double)product.Sellprice
        }).ToList();

        response.Data.AddRange(productList);
        return response;        
    }


    public override async Task<ProductReportResponse> ProductReport(ProductReportRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Descriptions, Qty, Unit, Costprice, Sellprice FROM products";

        var dbProducts = await dbConnection.QueryAsync<dynamic>(sql);

        if (dbProducts == null || !dbProducts.Any())
        {
            logger.LogWarning("No product data found in the database.");
            throw new RpcException(new Status(StatusCode.NotFound, "No product data found."));
        }

        var response = new ProductReportResponse();
        var productList = new List<ProductData>();

        foreach (var product in dbProducts)
        {

            productList.Add(new ProductData
            {
                Id = product.Id,
                Descriptions = product.Descriptions ?? string.Empty,
                Qty = product.Qty ?? 0,
                Unit = product.Unit ?? string.Empty,
                Costprice = (double)product.Costprice,
                Sellprice = (double)product.Sellprice
            });
        }

        response.Data.AddRange(productList);
        return response;
    }

    public override async Task<CategoryResponse> GetCategoryProducts(CategoryRequest request, ServerCallContext context)
    {
        const string sql = @"
            SELECT Id, Name FROM categories ORDER BY Id;
            SELECT Id, Category_id AS CategoryId, Descriptions, Qty, Unit, Costprice, Sellprice FROM products ORDER BY Category_id;";

        using var multi = await dbConnection.QueryMultipleAsync(sql);

        var categories = (await multi.ReadAsync<CategoryModel>()).ToList();
        var allProducts = (await multi.ReadAsync<DataProduct>()).ToList();

        if (categories == null || categories.Count == 0)
        {
            logger.LogWarning("No category data found.");
            throw new RpcException(new Status(StatusCode.NotFound, "No categories found."));
        }

        var productsByGroup = allProducts
            .GroupBy(p => p.CategoryId) 
            .ToDictionary(g => g.Key, g => g.ToList());

        var response = new CategoryResponse();

        foreach (var cat in categories)
        {
            var categoryWithProducts = new CategoryWithProducts
            {
                CategoryName = cat.Name
            };

            if (productsByGroup.TryGetValue(cat.Id, out var products))
            {
                categoryWithProducts.Products.AddRange(products);
            }

            response.Categories.Add(categoryWithProducts);
        }
        return response;
    }
}