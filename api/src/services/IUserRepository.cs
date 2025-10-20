using System;
using System.Collections.Generic;
using api.src.models;

namespace api.src.services
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User? GetUserByID(int id);
        User CreateUser(UserRequest userRequest);
        User? UpdateUser(int id, UserRequest userRequest);
    }
}