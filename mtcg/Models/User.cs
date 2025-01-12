using System;

namespace MTCG.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; } = 20;
        public int GamesPlayed { get; set; } = 0;
        public int GamesWon { get; set; } = 0;
        public int Elo { get; set; } = 100;
    }
}