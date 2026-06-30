using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

public class DbInitializer
{
    private readonly IConfiguration _configuration;

    public DbInitializer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
        var dbName = _configuration["DatabaseSettings:DbName"] ?? "core10_grpc";

        // 1. Build and sanitize connection strings safely
        var builder = new MySqlConnectionStringBuilder(baseConnectionString);
        builder.Database = ""; // Strip database to connect to server level
        var serverConnectionString = builder.ConnectionString;

        builder.Database = dbName; // Set target database
        var targetConnectionString = builder.ConnectionString;

        // 2. Create database if it does not exist
        using (var connection = new MySqlConnection(serverConnectionString))
        {
            await connection.OpenAsync();
            var createDbSql = $"CREATE DATABASE IF NOT EXISTS `{dbName}`;";
            await connection.ExecuteAsync(createDbSql);
        }

        // 3. Create tables using the target connection
        using (var connection = new MySqlConnection(targetConnectionString))
        {
            await connection.OpenAsync();
            await CreateTablesAsync(connection);
        }
    }

    private static async Task CreateTablesAsync(IDbConnection connection)
    {
        // Reordered: Independent tables first, then junction tables. Added IF NOT EXISTS.
        const string createUsersAndRolesTable = @"
            CREATE TABLE IF NOT EXISTS `users` (
              `id` int NOT NULL AUTO_INCREMENT,
              `created_at` datetime(3) DEFAULT CURRENT_TIMESTAMP(3),
              `updated_at` datetime(3) DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
              `deleted_at` datetime(3) DEFAULT NULL,
              `lastname` varchar(255) DEFAULT NULL,
              `firstname` varchar(255) DEFAULT NULL,
              `email` varchar(255) DEFAULT NULL,
              `mobile` varchar(255) DEFAULT NULL,
              `username` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
              `password` varchar(255) DEFAULT NULL,
              `isactivated` int DEFAULT '1',
              `isblocked` int DEFAULT '0',
              `userpicture` varchar(191) DEFAULT 'pix.png',
              `mailtoken` int DEFAULT '0',
              `secret` text,
              `qrcodeurl` text,
              `role_id` int DEFAULT NULL,
              PRIMARY KEY (`id`),
              UNIQUE KEY `idx_users_email` (`email`),
              UNIQUE KEY `idx_users_username` (`username`),
              KEY `idx_users_deleted_at` (`deleted_at`)
            ) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

            CREATE TABLE IF NOT EXISTS `roles` (
              `id` int NOT NULL AUTO_INCREMENT,
              `created_at` datetime(3) DEFAULT NULL,
              `updated_at` datetime(3) DEFAULT NULL,
              `deleted_at` datetime(3) DEFAULT NULL,
              `name` varchar(25) DEFAULT NULL,
              PRIMARY KEY (`id`),
              KEY `idx_roles_deleted_at` (`deleted_at`)
            ) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

            CREATE TABLE IF NOT EXISTS `user_roles` (
              `user_id` int NOT NULL,
              `role_id` int NOT NULL,
              PRIMARY KEY (`user_id`,`role_id`),
              KEY `fk_user_roles_role` (`role_id`),
              CONSTRAINT `fk_user_roles_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`),
              CONSTRAINT `fk_user_roles_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

        const string createProductsAndCategoriesTable = @"
            CREATE TABLE IF NOT EXISTS `products` (
              `id` int NOT NULL AUTO_INCREMENT,
              `created_at` datetime(3) DEFAULT CURRENT_TIMESTAMP(3),
              `updated_at` datetime(3) DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
              `deleted_at` datetime(3) DEFAULT NULL,
              `category` varchar(255) DEFAULT NULL,
              `descriptions` varchar(255) DEFAULT NULL,
              `qty` int DEFAULT '0',
              `unit` varchar(255) DEFAULT NULL,
              `costprice` decimal(10,2) DEFAULT '0.00',
              `sellprice` decimal(10,2) DEFAULT '0.00',
              `saleprice` decimal(10,2) DEFAULT '0.00',
              `productpicture` varchar(255) DEFAULT NULL,
              `alertstocks` int DEFAULT '0',
              `criticalstocks` int DEFAULT '0',
              `category_id` int DEFAULT NULL,
              `category_rel_id` bigint DEFAULT NULL,
              PRIMARY KEY (`id`),
              UNIQUE KEY `idx_products_descriptions` (`descriptions`),
              KEY `idx_products_deleted_at` (`deleted_at`)
            ) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

            CREATE TABLE IF NOT EXISTS `categories` (
              `id` int NOT NULL AUTO_INCREMENT,
              `created_at` datetime(3) DEFAULT NULL,
              `updated_at` datetime(3) DEFAULT NULL,
              `deleted_at` datetime(3) DEFAULT NULL,
              `name` varchar(25) DEFAULT NULL,
              PRIMARY KEY (`id`),
              KEY `idx_categories_deleted_at` (`deleted_at`)
            ) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

            CREATE TABLE IF NOT EXISTS `product_categories` (
              `category_id` int NOT NULL,
              `product_id` int NOT NULL,
              PRIMARY KEY (`category_id`,`product_id`),
              KEY `fk_product_categories_product` (`product_id`),
              CONSTRAINT `fk_product_categories_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`),
              CONSTRAINT `fk_product_categories_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

        const string createSalesTable = @"
            CREATE TABLE IF NOT EXISTS `sales` (
              `id` bigint NOT NULL AUTO_INCREMENT,
              `salesamount` decimal(10,2) DEFAULT '0.00',
              `salesdate` datetime(3) DEFAULT CURRENT_TIMESTAMP(3),
              PRIMARY KEY (`id`)
            ) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

        // Combine operations or run sequentially safely now
        await connection.ExecuteAsync(createUsersAndRolesTable);
        await connection.ExecuteAsync(createProductsAndCategoriesTable);
        await connection.ExecuteAsync(createSalesTable);
    }
}
