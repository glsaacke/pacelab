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
    public class ActivityWeatherController : ControllerBase
    {
        private readonly IActivityWeatherRepository _activityWeatherRepository;
        private readonly ILogger<ActivityWeatherController> _logger;
        
        public ActivityWeatherController(IActivityWeatherRepository activityWeatherRepository, ILogger<ActivityWeatherController> logger)
        {
            _activityWeatherRepository = activityWeatherRepository;
            _logger = logger;
        }

        [HttpGet("GetAllActivityWeathers")]
        public IActionResult GetAllActivityWeathers()
        {
            try{
                var activityWeathers = _activityWeatherRepository.GetAllActivityWeathers();

                if (activityWeathers == null || activityWeathers.Count == 0)
                {
                    _logger.LogWarning("No activity weather records found.");
                    return NotFound(new { Message = "No activity weather records found." });
                } else {
                    return Ok(activityWeathers);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching all activity weather records.");
                throw;
            }   
        }

        [HttpGet("GetActivityWeatherById/{id}")]
        public IActionResult GetActivityWeatherById(int id)
        {
            ActivityWeather? activityWeather;
            try{
                activityWeather = _activityWeatherRepository.GetActivityWeatherById(id);

                if (activityWeather == null){
                    _logger.LogWarning("No activity weather record found with id: {Id}", id);
                    return NotFound(new { Message = "No activity weather record found." });
                } else {
                    return Ok(activityWeather);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching activity weather record.");
                throw;
            }
        }

        [HttpGet("GetActivityWeatherByActivityId/{activityId}")]
        public IActionResult GetActivityWeatherByActivityId(int activityId)
        {
            ActivityWeather? activityWeather;
            try{
                activityWeather = _activityWeatherRepository.GetActivityWeatherByActivityId(activityId);

                if (activityWeather == null){
                    _logger.LogWarning("No activity weather record found for activity id: {ActivityId}", activityId);
                    return NotFound(new { Message = "No activity weather record found for this activity." });
                } else {
                    return Ok(activityWeather);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching activity weather record by activity id.");
                throw;
            }
        }

        [HttpPost("CreateActivityWeather")]
        public IActionResult CreateActivityWeather([FromBody] ActivityWeatherRequest activityWeatherRequest)
        {
            try{
                if (activityWeatherRequest == null)
                {
                    return BadRequest(new { Message = "Activity weather data is required." });
                }

                var createdActivityWeather = _activityWeatherRepository.CreateActivityWeather(activityWeatherRequest);
                return CreatedAtAction(nameof(GetActivityWeatherById), new { id = createdActivityWeather.ActivityWeatherId }, createdActivityWeather);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while creating activity weather record.");
                throw;
            }
        }

        [HttpPut("UpdateActivityWeather/{id}")]
        public IActionResult UpdateActivityWeather(int id, [FromBody] ActivityWeatherRequest activityWeatherRequest)
        {
            try{
                if (activityWeatherRequest == null)
                {
                    return BadRequest(new { Message = "Activity weather data is required." });
                }

                var updatedActivityWeather = _activityWeatherRepository.UpdateActivityWeather(id, activityWeatherRequest);
                if (updatedActivityWeather == null)
                {
                    _logger.LogWarning("Activity weather record with id: {Id} not found for update", id);
                    return NotFound(new { Message = "Activity weather record not found." });
                }

                return Ok(updatedActivityWeather);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while updating activity weather record.");
                throw;
            }
        }

        [HttpDelete("DeleteActivityWeather/{id}")]
        public IActionResult DeleteActivityWeather(int id)
        {
            try{
                var deleted = _activityWeatherRepository.DeleteActivityWeather(id);
                if (!deleted)
                {
                    _logger.LogWarning("Activity weather record with id: {Id} not found for deletion", id);
                    return NotFound(new { Message = "Activity weather record not found." });
                }

                return Ok(new { Message = "Activity weather record deleted successfully." });
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while deleting activity weather record.");
                throw;
            }
        }
    }
}