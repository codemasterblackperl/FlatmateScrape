using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flatmate
{
    public class Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int failed { get; set; }
    }

    public class Hits
    {
        public int total { get; set; }
        public double max_score { get; set; }
        public List<object> hits { get; set; }
    }

    public class Contexts
    {
        public List<string> location_type { get; set; }
    }

    public class Suggest2
    {
        public List<string> input { get; set; }
        public int weight { get; set; }
        public Contexts contexts { get; set; }
    }

    public class Source
    {
        public int id { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string suburb { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public List<object> polygon { get; set; }
        public string location_type { get; set; }
        public string key { get; set; }
        public int average_rent { get; set; }
        public object temp_latitude { get; set; }
        public object temp_longitude { get; set; }
        public int radius { get; set; }
        public object name { get; set; }
        public object short_name { get; set; }
        public List<object> synonyms { get; set; }
        public List<double> location { get; set; }
        public string search_title { get; set; }
        public string short_title { get; set; }
        public Suggest2 suggest { get; set; }
    }

    public class Contexts2
    {
        public List<string> location_type { get; set; }
    }

    public class Option
    {
        public string text { get; set; }
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public Source _source { get; set; }
        public Contexts2 contexts { get; set; }
    }

    public class LocationSuggest
    {
        public string text { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
        public List<Option> options { get; set; }
    }

    public class Suggest
    {
        public List<LocationSuggest> location_suggest { get; set; }
    }

    public class JsonLocation
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hits hits { get; set; }
        public Suggest suggest { get; set; }
    }
}
