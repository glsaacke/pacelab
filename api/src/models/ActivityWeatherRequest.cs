using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.src.models
{
    public class ActivityWeatherRequest
    {
        public int ActivityId { get; set; }
        public float Temperature { get; set; }
        public float HumidityPct { get; set; }
        public float WindSpeed { get; set; }
        public float Pressure { get; set; }
        public float FeelsLike { get; set; }
        public required string WeatherDescription { get; set; }
        public float Esi { get; set; }
        public int AdjustedPaceS { get; set; }
    }
}