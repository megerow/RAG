// -------------------------------------------------------------------------------
// Purpose: The following classes represent the structure returned by 
//          the WEATHER API.
//
// By:      Mark Gerow
// Date:    4/29/2024
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG6
{
    public class Geo
    {
        public string name { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }

    public class WeatherClouds
    {
        public int all { get; set; }
    }

    public class WeatherCoord

    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class WeatherMain
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class WeatherRoot
    {
        public WeatherCoord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public WeatherMain main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public WeatherClouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public int deg { get; set; }
    }
}
