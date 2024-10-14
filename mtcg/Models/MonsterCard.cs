namespace MTCG.Models
{
    public class MonsterCard : Card
    {
        public MonsterCard(string name, string type, int damage) : base(name, type, damage)
        {
            IsMonster = true;
        }
    }
}