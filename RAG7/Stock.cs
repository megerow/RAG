// -------------------------------------------------------------------------------
// Purpose: Classes to work with the FinnHub stock API.
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
    /// <summary>
    /// A price quote returned by the FinnHub API
    /// </summary>
    public class Quote
    {
        public double c { get; set; }
        public double CurrentPrice { get { return c; } }
        public double d { get; set; }
        public double Change { get { return d; } }
        public double dp { get; set; }
        public double PercentChange { get { return dp; } }
        public double h { get; set; }
        public double High { get { return h; } }
        public double l { get; set; }
        public double Low { get { return l; } }
        public double o { get; set; }
        public double Open { get { return o; } }
        public double pc { get; set; }
        public double PreviousClose { get { return pc; } }
        public int t { get; set; }
    }

    /// <summary>
    /// Represents a single stock ticker symbol.
    /// </summary>
    public class Symbol
    {
        public string currency { get; set; }
        public string description { get; set; }
        public string displaySymbol { get; set; }
        public string figi { get; set; }
        public object isin { get; set; }
        public string mic { get; set; }
        public string shareClassFIGI { get; set; }
        public string symbol { get; set; }
        public string symbol2 { get; set; }
        public string type { get; set; }
    }
}
