using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string OwnerFK { get; set; }
        public string Shareuser { get; set; }
    }
}