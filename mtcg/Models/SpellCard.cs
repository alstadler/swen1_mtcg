namespace MTCG.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string name, string type, int damage) : base(name, type, damage)
        {
            IsMonster = false;
        }
    }
}