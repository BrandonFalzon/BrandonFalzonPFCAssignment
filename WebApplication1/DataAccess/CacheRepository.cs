using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;
using StackExchange.Redis;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class CacheRepository
    {

        private IDatabase db;
        public CacheRepository()
        {
            // var connection = ConnectionMultiplexer.Connect("localhost"); //localhost if cache server is installed locally after downloaded from https://github.com/rgl/redis/downloads 
            // connection to your REDISLABS.com db as in the next line NOTE: DO NOT USE MY CONNECTION BECAUSE I HAVE A LIMIT OF 30MB...CREATE YOUR OWN ACCOUNT
            var connection = ConnectionMultiplexer.Connect("redis-15612.c233.eu-west-1-1.ec2.cloud.redislabs.com:15612,password=VfTlNM3i7Kmcr1TbxyntXyoiPjx6nAvx"); //<< connection here should be to your redis database from REDISLABS.COM
            db = connection.GetDatabase();
        }

        /// <summary>
        /// store a list of products in cache
        /// </summary>
        /// <param name="products"></param>
        public void UpdateCache(List<Product> products)
        {
            if (db.KeyExists("products") == true)
                db.KeyDelete("products");

            string jsonString;
            jsonString = JsonSerializer.Serialize(products);

            db.StringSet("products", jsonString);
        }
        /// <summary>
        /// Gets a list of products from cache
        /// </summary>
        /// <returns></returns>
        public List<Product> GetProductsFromCache()
        {
            if (db.KeyExists("products") == true)
            {
                var products = JsonSerializer.Deserialize<List<Product>>(db.StringGet("products"));
                return products;
            }
            else
            {
                return new List<Product>();
            }
        }
    }
}