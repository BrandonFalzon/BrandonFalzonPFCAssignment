using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class ProductsRepository: ConnectionClass
    {
        public ProductsRepository(): base() { }

        public List<Product> GetProducts()
        {
            string sql = "Select Id, Name, File, Ownerfk, Shareuser from products ";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);

            MyConnection.Open();

            List<Product> results = new List<Product>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product p = new Product();
                    p.Id = reader.GetInt32(0);
                    p.Name = reader.GetString(1);
                    p.File = reader.GetString(2);
                    p.OwnerFK = reader.GetString(3);
                    p.Shareuser = reader.GetString(4);
                    results.Add(p);
                }
            }

            MyConnection.Close();

            return results;
        }

        public List<Product> GetProducts(string email)
        {
            string sql = "Select Id, Name, File, Ownerfk, Shareuser from products where Ownerfk = @email or Shareuser = @email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();

            List<Product> results = new List<Product>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product p = new Product();
                    p.Id = reader.GetInt32(0);
                    p.Name = reader.GetString(1);
                    p.File = reader.GetString(2);
                    p.OwnerFK = reader.GetString(3);
                    p.Shareuser = reader.GetString(4);
                    results.Add(p);
                }
            }

            MyConnection.Close();

            return results;
        }


        public void DeleteProduct(int id)
        {
            string sql = "Delete from products where Id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);

            bool connectionOpenedInThisMethod = false;

            if(MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
                connectionOpenedInThisMethod = true;
            }

            if (MyTransaction != null)
            {
                cmd.Transaction = MyTransaction; //to participate in the opened transaction (somwhere else), assign the property to the opened transaction
            }
            
            cmd.ExecuteNonQuery();

            if (connectionOpenedInThisMethod == true)
            {
                MyConnection.Close();
            }
        }

        public void AddProduct(Product p)//string name, string file, string ownerfk)
        {
            string sql = "INSERT into products (name, file, ownerfk, shareuser) values (@name, @file, @ownerfk, @shareuser)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@file", p.File);
            cmd.Parameters.AddWithValue("@ownerfk", p.OwnerFK);
            cmd.Parameters.AddWithValue("@shareuser", p.Shareuser);

            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }
        //public List<Product> AddProduct(Product p)
        //{
        //    string sql = "Select Id, Name, File, Ownerfk, Shareuser from products ";

        //    NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);

        //    MyConnection.Open();

        //    List<Product> results = new List<Product>();

        //    using (var reader = cmd.ExecuteReader())
        //    {
        //        while (reader.Read())
        //        {
        //            //Product p = new Product();
        //            p.Id = reader.GetInt32(0);
        //            p.Name = reader.GetString(1);
        //            //p.File = Link;
        //            p.OwnerFK = reader.GetString(3);
        //            p.Shareuser = reader.GetString(4);
        //            results.Add(p);
        //        }
        //    }

        //    MyConnection.Close();

        //    return results;
        //}
    }
}