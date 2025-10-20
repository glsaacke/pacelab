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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        
        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            try{
                var users = _userRepository.GetAllUsers();

                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning("No users found.");
                    return NotFound(new { Message = "No users found." });
                } else {
                    return Ok(users);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching all users.");
                throw;
            }   
        }

        [HttpGet("GetUserById/{id}")]
        public IActionResult GetUserById(int id)
        {
            User? user;
            try{
                user = _userRepository.GetUserByID(id);

                if (user == null){
                    _logger.LogWarning("No user found with id: {Id}", id);
                    return NotFound(new { Message = "No user found." });
                } else {
                    return Ok(user);
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while fetching user.");
                throw;
            }
        }

        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] UserRequest userRequest)
        {
            try{
                if (userRequest == null)
                {
                    return BadRequest(new { Message = "User data is required." });
                }

                var createdUser = _userRepository.CreateUser(userRequest);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserId }, createdUser);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while creating user.");
                throw;
            }
        }

        [HttpPut("UpdateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserRequest userRequest)
        {
            try{
                if (userRequest == null)
                {
                    return BadRequest(new { Message = "User data is required." });
                }

                var updatedUser = _userRepository.UpdateUser(id, userRequest);
                if (updatedUser == null)
                {
                    _logger.LogWarning("User with id: {Id} not found for update", id);
                    return NotFound(new { Message = "User not found." });
                }

                return Ok(updatedUser);
            }
            catch(Exception ex){
                _logger.LogError(ex, "An error occurred while updating user.");
                throw;
            }
        }
    }
}