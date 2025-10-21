using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.src.models;
using api.src.data;

namespace api.src.repositories
{
    public class ActivityWeatherRepository : IActivityWeatherRepository
    {
        private readonly PaceLabContext _context;
        
        public ActivityWeatherRepository(PaceLabContext context)
        {
            _context = context;
        }

        public List<ActivityWeather> GetAllActivityWeathers()
        {
            return _context.ActivityWeathers.ToList();
        }

        public ActivityWeather? GetActivityWeatherById(int id)
        {
            return _context.ActivityWeathers.FirstOrDefault(aw => aw.ActivityWeatherId == id);
        }

        public ActivityWeather? GetActivityWeatherByActivityId(int activityId)
        {
            return _context.ActivityWeathers.FirstOrDefault(aw => aw.ActivityId == activityId);
        }

        public ActivityWeather CreateActivityWeather(ActivityWeatherRequest activityWeatherRequest)
        {
            var activityWeather = new ActivityWeather
            {
                ActivityId = activityWeatherRequest.ActivityId,
                Temperature = activityWeatherRequest.Temperature,
                HumidityPct = activityWeatherRequest.HumidityPct,
                WindSpeed = activityWeatherRequest.WindSpeed,
                Pressure = activityWeatherRequest.Pressure,
                FeelsLike = activityWeatherRequest.FeelsLike,
                WeatherDescription = activityWeatherRequest.WeatherDescription,
                Esi = activityWeatherRequest.Esi,
                AdjustedPaceS = activityWeatherRequest.AdjustedPaceS
            };

            _context.ActivityWeathers.Add(activityWeather);
            _context.SaveChanges();
            return activityWeather;
        }

        public ActivityWeather? UpdateActivityWeather(int id, ActivityWeatherRequest activityWeatherRequest)
        {
            var existingActivityWeather = _context.ActivityWeathers.FirstOrDefault(aw => aw.ActivityWeatherId == id);
            if (existingActivityWeather == null)
                return null;

            existingActivityWeather.ActivityId = activityWeatherRequest.ActivityId;
            existingActivityWeather.Temperature = activityWeatherRequest.Temperature;
            existingActivityWeather.HumidityPct = activityWeatherRequest.HumidityPct;
            existingActivityWeather.WindSpeed = activityWeatherRequest.WindSpeed;
            existingActivityWeather.Pressure = activityWeatherRequest.Pressure;
            existingActivityWeather.FeelsLike = activityWeatherRequest.FeelsLike;
            existingActivityWeather.WeatherDescription = activityWeatherRequest.WeatherDescription;
            existingActivityWeather.Esi = activityWeatherRequest.Esi;
            existingActivityWeather.AdjustedPaceS = activityWeatherRequest.AdjustedPaceS;

            _context.SaveChanges();
            return existingActivityWeather;
        }

        public bool DeleteActivityWeather(int id)
        {
            var activityWeather = _context.ActivityWeathers.FirstOrDefault(aw => aw.ActivityWeatherId == id);
            if (activityWeather == null)
                return false;

            _context.ActivityWeathers.Remove(activityWeather);
            _context.SaveChanges();
            return true;
        }
    }
}