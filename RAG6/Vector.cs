using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG6
{
    // Classes below are used to add data to the vector database.

    public class Customer
    {
        public string Name { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class SalesRep
    {
        public string Name { get; set; }
    }
}
