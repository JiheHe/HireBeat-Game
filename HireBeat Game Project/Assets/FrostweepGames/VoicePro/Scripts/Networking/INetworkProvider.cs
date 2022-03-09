using System;

namespace FrostweepGames.VoicePro
{
    public interface INetworkProvider
    {
        event Action<INetworkActor, byte[]> NetworkDataReceivedEvent;
        event Action<INetworkActor, NetworkRouter.NetworkCommand> NetworkCommandReceivedEvent;

        bool ReadyToTransmit { get; }
        INetworkActor NetworkActor { get; }

        void Init(INetworkActor networkActor);
        void Dispose();
        void SendNetworkData(NetworkRouter.NetworkParameters parameters, byte[] bytes);
        void SendCommandData(NetworkRouter.NetworkCommand command);
        string GetNetworkState();
        string GetConnectionToServer();
        string GetCurrentRoomName();
    }
}