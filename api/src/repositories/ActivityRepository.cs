using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.src.models;
using api.src.data;

namespace api.src.repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly PaceLabContext _context;
        
        public ActivityRepository(PaceLabContext context)
        {
            _context = context;
        }

        public List<Activity> GetAllActivities()
        {
            return _context.Activities.ToList();
        }

        public Activity? GetActivityById(int id)
        {
            return _context.Activities.FirstOrDefault(a => a.ActivityId == id);
        }

        public List<Activity> GetActivitiesByUserId(int userId)
        {
            return _context.Activities.Where(a => a.UserId == userId).ToList();
        }

        public Activity CreateActivity(ActivityRequest activityRequest)
        {
            var activity = new Activity
            {
                UserId = activityRequest.UserId,
                Name = activityRequest.Name,
                Type = activityRequest.Type,
                StartDate = activityRequest.StartDate,
                DistanceM = activityRequest.DistanceM,
                MovingTimeS = activityRequest.MovingTimeS,
                ElevationGainM = activityRequest.ElevationGainM,
                AverageSpeedMph = activityRequest.AverageSpeedMph,
                StartLatitude = activityRequest.StartLatitude,
                StartLongitude = activityRequest.StartLongitude
            };

            _context.Activities.Add(activity);
            _context.SaveChanges();
            return activity;
        }

        public Activity? UpdateActivity(int id, ActivityRequest activityRequest)
        {
            var existingActivity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
            if (existingActivity == null)
                return null;

            existingActivity.UserId = activityRequest.UserId;
            existingActivity.Name = activityRequest.Name;
            existingActivity.Type = activityRequest.Type;
            existingActivity.StartDate = activityRequest.StartDate;
            existingActivity.DistanceM = activityRequest.DistanceM;
            existingActivity.MovingTimeS = activityRequest.MovingTimeS;
            existingActivity.ElevationGainM = activityRequest.ElevationGainM;
            existingActivity.AverageSpeedMph = activityRequest.AverageSpeedMph;
            existingActivity.StartLatitude = activityRequest.StartLatitude;
            existingActivity.StartLongitude = activityRequest.StartLongitude;

            _context.SaveChanges();
            return existingActivity;
        }

        public bool DeleteActivity(int id)
        {
            var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
            if (activity == null)
                return false;

            _context.Activities.Remove(activity);
            _context.SaveChanges();
            return true;
        }
    }
}