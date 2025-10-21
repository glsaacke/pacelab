using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using api.src.models;
using api.src.repositories;
using Microsoft.Extensions.Logging;

namespace api.core.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<ActivityController> _logger;
        
        public ActivityController(IActivityRepository activityRepository, ILogger<ActivityController> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        [HttpGet("GetAllActivities")]
        public IActionResult GetAllActivities()
        {
            try{
                var activities = _activityRepository.GetAllActivities();

                if (activities == null || activities.Count == 0)
                {
                    _logger.LogWarning("No activities found.");
                    return NotFound(new { Message = "No activities found." });
                } else {
                    return Ok(activities);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching all activities.");
                throw;
            }   
        }

        [HttpGet("GetActivityById/{id}")]
        public IActionResult GetActivityById(int id)
        {
            Activity? activity;
            try{
                activity = _activityRepository.GetActivityById(id);

                if (activity == null){
                    _logger.LogWarning("No activity found with id: {Id}", id);
                    return NotFound(new { Message = "No activity found." });
                } else {
                    return Ok(activity);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching activity.");
                throw;
            }
        }

        [HttpGet("GetActivitiesByUserId/{userId}")]
        public IActionResult GetActivitiesByUserId(int userId)
        {
            List<Activity> activities;
            try{
                activities = _activityRepository.GetActivitiesByUserId(userId);

                if (activities == null || activities.Count == 0){
                    _logger.LogWarning("No activities found for user id: {UserId}", userId);
                    return NotFound(new { Message = "No activities found for this user." });
                } else {
                    return Ok(activities);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching activities by user id.");
                throw;
            }
        }

        [HttpPost("CreateActivity")]
        public IActionResult CreateActivity([FromBody] ActivityRequest activityRequest)
        {
            try{
                if (activityRequest == null)
                {
                    return BadRequest(new { Message = "Activity data is required." });
                }

                var createdActivity = _activityRepository.CreateActivity(activityRequest);
                return CreatedAtAction(nameof(GetActivityById), new { id = createdActivity.ActivityId }, createdActivity);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while creating activity.");
                throw;
            }
        }

        [HttpPut("UpdateActivity/{id}")]
        public IActionResult UpdateActivity(int id, [FromBody] ActivityRequest activityRequest)
        {
            try{
                if (activityRequest == null)
                {
                    return BadRequest(new { Message = "Activity data is required." });
                }

                var updatedActivity = _activityRepository.UpdateActivity(id, activityRequest);
                if (updatedActivity == null)
                {
                    _logger.LogWarning("Activity with id: {Id} not found for update", id);
                    return NotFound(new { Message = "Activity not found." });
                }

                return Ok(updatedActivity);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while updating activity.");
                throw;
            }
        }

        [HttpDelete("DeleteActivity/{id}")]
        public IActionResult DeleteActivity(int id)
        {
            try{
                var deleted = _activityRepository.DeleteActivity(id);
                if (!deleted)
                {
                    _logger.LogWarning("Activity with id: {Id} not found for deletion", id);
                    return NotFound(new { Message = "Activity not found." });
                }

                return Ok(new { Message = "Activity deleted successfully." });
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while deleting activity.");
                throw;
            }
        }
    }
}