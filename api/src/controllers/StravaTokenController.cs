using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using api.src.models;
using api.src.services;
using Microsoft.Extensions.Logging;

namespace api.core.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StravaTokenController : ControllerBase
    {
        private readonly IStravaTokenRepository _stravaTokenRepository;
        private readonly ILogger<StravaTokenController> _logger;
        
        public StravaTokenController(IStravaTokenRepository stravaTokenRepository, ILogger<StravaTokenController> logger)
        {
            _stravaTokenRepository = stravaTokenRepository;
            _logger = logger;
        }

        [HttpGet("GetAllStravaTokens")]
        public IActionResult GetAllStravaTokens()
        {
            try{
                var stravaTokens = _stravaTokenRepository.GetAllStravaTokens();

                if (stravaTokens == null || stravaTokens.Count == 0)
                {
                    _logger.LogWarning("No Strava tokens found.");
                    return NotFound(new { Message = "No Strava tokens found." });
                } else {
                    return Ok(stravaTokens);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching all Strava tokens.");
                throw;
            }   
        }

        [HttpGet("GetStravaTokenById/{id}")]
        public IActionResult GetStravaTokenById(int id)
        {
            StravaToken? stravaToken;
            try{
                stravaToken = _stravaTokenRepository.GetStravaTokenById(id);

                if (stravaToken == null){
                    _logger.LogWarning("No Strava token found with id: {Id}", id);
                    return NotFound(new { Message = "No Strava token found." });
                } else {
                    return Ok(stravaToken);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching Strava token.");
                throw;
            }
        }

        [HttpGet("GetStravaTokenByUserId/{userId}")]
        public IActionResult GetStravaTokenByUserId(int userId)
        {
            StravaToken? stravaToken;
            try{
                stravaToken = _stravaTokenRepository.GetStravaTokenByUserId(userId);

                if (stravaToken == null){
                    _logger.LogWarning("No Strava token found for user id: {UserId}", userId);
                    return NotFound(new { Message = "No Strava token found for this user." });
                } else {
                    return Ok(stravaToken);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching Strava token by user id.");
                throw;
            }
        }

        [HttpPost("CreateStravaToken")]
        public IActionResult CreateStravaToken([FromBody] StravaTokenRequest stravaTokenRequest)
        {
            try{
                if (stravaTokenRequest == null)
                {
                    return BadRequest(new { Message = "Strava token data is required." });
                }

                var createdStravaToken = _stravaTokenRepository.CreateStravaToken(stravaTokenRequest);
                return CreatedAtAction(nameof(GetStravaTokenById), new { id = createdStravaToken.StravaTokenId }, createdStravaToken);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while creating Strava token.");
                throw;
            }
        }

        [HttpPut("UpdateStravaToken/{id}")]
        public IActionResult UpdateStravaToken(int id, [FromBody] StravaTokenRequest stravaTokenRequest)
        {
            try{
                if (stravaTokenRequest == null)
                {
                    return BadRequest(new { Message = "Strava token data is required." });
                }

                var updatedStravaToken = _stravaTokenRepository.UpdateStravaToken(id, stravaTokenRequest);
                if (updatedStravaToken == null)
                {
                    _logger.LogWarning("Strava token with id: {Id} not found for update", id);
                    return NotFound(new { Message = "Strava token not found." });
                }

                return Ok(updatedStravaToken);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while updating Strava token.");
                throw;
            }
        }

        [HttpDelete("DeleteStravaToken/{id}")]
        public IActionResult DeleteStravaToken(int id)
        {
            try{
                var deleted = _stravaTokenRepository.DeleteStravaToken(id);
                if (!deleted)
                {
                    _logger.LogWarning("Strava token with id: {Id} not found for deletion", id);
                    return NotFound(new { Message = "Strava token not found." });
                }

                return Ok(new { Message = "Strava token deleted successfully." });
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while deleting Strava token.");
                throw;
            }
        }
    }
}