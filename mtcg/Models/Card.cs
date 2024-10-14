using MTCG.Interfaces;

namespace MTCG.Models
{
    public abstract class Card : ICard
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Damage { get; set; }
        public bool IsMonster { get; protected set; }

        public Card(string name, string type, int damage)
        {
            Name = name;
            Type = type;
            Damage = damage;
        }
    }
}