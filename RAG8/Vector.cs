// -------------------------------------------------------------------------------
// Purpose: Classes below are used to add data to the vector database.
//
// By:      Mark Gerow
// Date:    4/29/2024
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG
{
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
