using MTCG.Interfaces;
using MTCG.Models;
using MTCG.DTOs;
using Newtonsoft.Json;

namespace MTCG.Controllers
{
    public class UserController
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public string RegisterUser(string username, string password)
        {
            var user = _userService.Register(username, password);
            return JsonConvert.SerializeObject(new { Message = $"User {user.Username} successfully registered." });
        }

        public string Login(string username, string password)
        {
            string token = _userService.Login(username, password);
            return JsonConvert.SerializeObject(new { Token = token });
        }
    }
}