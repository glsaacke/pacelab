using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.src.models;
using api.src.data;

namespace api.src.services
{
    public class UserRepository : IUserRepository
    {
        private readonly PaceLabContext _context;
        
        public UserRepository(PaceLabContext context)
        {
            _context = context;
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public User? GetUserByID(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        public User CreateUser(UserRequest userRequest)
        {
            var user = new User
            {
                StravaId = userRequest.StravaId,
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User? UpdateUser(int id, UserRequest userRequest)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (existingUser == null)
                return null;

            existingUser.StravaId = userRequest.StravaId;
            existingUser.FirstName = userRequest.FirstName;
            existingUser.LastName = userRequest.LastName;
            // Don't update CreatedAt or LastLogin

            _context.SaveChanges();
            return existingUser;
        }
    }
}