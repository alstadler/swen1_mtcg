using System;

namespace MTCG.Models
{
    public class Trade
    {
        public Guid Id { get; set; }
        public string CardToTrade { get; set; }
        public string WantedType { get; set; }
        public float MinimumDamage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}