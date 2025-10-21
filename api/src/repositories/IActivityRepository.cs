using System;
using System.Collections.Generic;
using api.src.models;

namespace api.src.repositories
{
    public interface IActivityRepository
    {
        List<Activity> GetAllActivities();
        Activity? GetActivityById(int id);
        List<Activity> GetActivitiesByUserId(int userId);
        Activity CreateActivity(ActivityRequest activityRequest);
        Activity? UpdateActivity(int id, ActivityRequest activityRequest);
        bool DeleteActivity(int id);
    }
}