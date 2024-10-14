using System.Collections.Generic;

namespace MTCG.Models
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Token { get; set; }
        public int Coins { get; set; }
        public List<Card> CardStack { get; set; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;    // To-Do: Password Hashing
            Coins = 20;     // Initial Coins set to 20
            CardStack = new List<Card>();
        }
    }
}