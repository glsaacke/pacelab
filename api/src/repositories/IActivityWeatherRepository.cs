using System;
using System.Collections.Generic;
using api.src.models;

namespace api.src.repositories
{
    public interface IActivityWeatherRepository
    {
        List<ActivityWeather> GetAllActivityWeathers();
        ActivityWeather? GetActivityWeatherById(int id);
        ActivityWeather? GetActivityWeatherByActivityId(int activityId);
        ActivityWeather CreateActivityWeather(ActivityWeatherRequest activityWeatherRequest);
        ActivityWeather? UpdateActivityWeather(int id, ActivityWeatherRequest activityWeatherRequest);
        bool DeleteActivityWeather(int id);
    }
}