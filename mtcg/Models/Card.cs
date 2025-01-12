using System;

namespace MTCG.Models
{
    public class Card
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float Damage { get; set; }

        public string Type { get; set; }
    }
}