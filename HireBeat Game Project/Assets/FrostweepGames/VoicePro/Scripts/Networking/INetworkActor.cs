namespace FrostweepGames.VoicePro
{
    public interface INetworkActor
    {
        string Id { get; }
        string Name { get; }
        bool IsAdmin { get; }
        NetworkActorInfo Info { get; }

        public void ChangeNetworkInfoName(string name) //custom method by me
        {
            Info.name = name;
        }
    }
}