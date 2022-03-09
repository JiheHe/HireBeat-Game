namespace FrostweepGames.VoicePro
{
    public interface INetworkActor
    {
        string Id { get; }
        string Name { get; }
        bool IsAdmin { get; }
        NetworkActorInfo Info { get; }
    }
}