namespace MTCG.Interfaces
{
    public interface ICard
    {
        string Name { get; }
        string Type { get; }
        int Damage { get; }
        bool IsMonster { get; }
    }
}
