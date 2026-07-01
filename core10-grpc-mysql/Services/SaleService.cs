// Services/SaleService.cs
using System.Data;
using Dapper;
using Grpc.Core;

namespace core10_grpc_mysql.Services;

public class SaleService(ILogger<SaleService> logger, IDbConnection dbConnection) : Sale.SaleBase
{
    public override async Task<SalesListResponse> SalesList(SalesListRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Salesamount, Salesdate FROM sales";

        var dbSales = await dbConnection.QueryAsync<dynamic>(sql);

        if (dbSales == null || !dbSales.Any())
        {
            logger.LogWarning("No sales data found in the database.");
            throw new RpcException(new Status(StatusCode.NotFound, "No sales data found."));
        }

        var response = new SalesListResponse();
        var saleList = new List<SalesData>();

        foreach (var sale in dbSales)
        {
            var amount = sale.Salesamount != null ? Convert.ToDouble(sale.Salesamount) : 0.0;
            string formattedDate = null;
            if (sale.Salesdate != null)
            {
                DateTime dateValue = Convert.ToDateTime(sale.Salesdate);
                formattedDate = dateValue.ToString("yyyy-MM-dd");
            }

            saleList.Add(new SalesData
            {
                Salesamount = amount,
                Salesdate = formattedDate
            });
        }

        response.Data.AddRange(saleList);
        return response;
    }
}
