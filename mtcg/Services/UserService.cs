using MTCG.Interfaces;
using MTCG.Models;
using MTCG.Utilities;
using System;
using System.Collections.Generic;

namespace MTCG.Services
{
    public class UserService : IUserService
    {
        private Dictionary<string, User> 
        _users = new Dictionary<string, User>();

        public User Register(string username, string password)
        {
            if (_users.ContainsKey(username))
                throw new Exception("User already exists.");

            var user = new User(username, password);
            _users[username] = user;
            return user;
        }

        public User GetUserByToken(string token)
        {
            foreach (var user in _users.Values)
            {
                if (user.Token == token)
                    return user;
            }
            return null;
        }

        public string Login(string username, string password)
        {
            if (!_users.ContainsKey(username) || (_users[username].Password != password))
                throw new Exception("Invalid credentials.");

            string token = TokenManager.GenerateToken(username);
            _users[username].Token = token;
            return token;
        }
    }
}