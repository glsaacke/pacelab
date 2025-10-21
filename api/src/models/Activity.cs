using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.src.models
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public DateTime StartDate { get; set; }
        public float DistanceM { get; set; }
        public int MovingTimeS { get; set; }
        public float ElevationGainM { get; set; }
        public float AverageSpeedMph { get; set; }
        public float? StartLatitude { get; set; }
        public float? StartLongitude { get; set; }
    }
}