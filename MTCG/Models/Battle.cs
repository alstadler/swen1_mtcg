using System;

namespace MTCG.Models
{
    public class Battle
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public string Result { get; set; } // "win", "lose", "draw"
        public string BattleLog { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}