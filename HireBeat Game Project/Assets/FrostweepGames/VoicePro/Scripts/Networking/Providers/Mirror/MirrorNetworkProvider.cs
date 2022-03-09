#if MIRROR_NETWORK_PROVIDER
using Mirror;
using System;
using System.Collections.Generic;

namespace FrostweepGames.VoicePro.NetworkProviders.Mirror
{
    /// <summary>
    /// Mirror Network Provider for data transmition
    /// </summary>
    public class MirrorNetworkProvider : INetworkProvider
    {
        public event Action<INetworkActor, byte[]> NetworkDataReceivedEvent;

        public event Action<INetworkActor, NetworkRouter.NetworkCommand> NetworkCommandReceivedEvent;

        private INetworkActor _networkActor;

        public bool ReadyToTransmit => NetworkClient.isConnected;

        public INetworkActor NetworkActor => _networkActor;

        public void Dispose()
        {
            NetworkClient.UnregisterHandler<TransportVoiceMessage>();
            NetworkClient.UnregisterHandler<TransportCommandMessage>();

            _networkActor = null;
            NetworkDataReceivedEvent = null;
        }

        public void Init(INetworkActor networkActor)
        {
            _networkActor = networkActor;

            NetworkClient.RegisterHandler<TransportVoiceMessage>(NetworkEventReceivedHandler);
            NetworkClient.RegisterHandler<TransportCommandMessage>(NetworkCommandEventReceivedHandler);
        }

        public void SendNetworkData(NetworkRouter.NetworkParameters parameters, byte[] bytes)
        {
            TransportVoiceMessage message = new TransportVoiceMessage()
            {
               sender = _networkActor.ToString(),
               bytes = bytes,
               targetAll = parameters.sendToAll
            };
            NetworkClient.connection.Send(message);
        }

        public void SendCommandData(NetworkRouter.NetworkCommand command)
        {
            TransportCommandMessage message = new TransportCommandMessage()
            {
                sender = _networkActor.ToString(),
                data = Plugins.SimpleJSON.JSONEncoder.Encode(new Dictionary<string, object>()
                {
                    { "command", command.command.ToString() },
                    { "data", command.data },
                }),
                targetAll = true
            };
            NetworkClient.connection.Send(message);
        }

        public string GetNetworkState()
        {
            return NetworkClient.isConnected ? "Connected" : "Disconnected";
        }

        public string GetConnectionToServer()
        {
            return NetworkClient.serverIp;
        }

        public string GetCurrentRoomName()
        {
            return NetworkClient.serverIp;
        }

        /// <summary>
        /// Uses for setting of admin rights
        /// </summary>
        /// <returns></returns>
        public bool SetAdminOfRoom()
        {
            if (!ReadyToTransmit)
                return false;

            // NEED TO BE IMPROVED SOMEHOW BASED ON SERVER SETTINGS.

            ((MirrorNetworkActor)NetworkActor).SetAdminStatus(true);

            return true;
        }

        /// <summary>
        /// event handler of network events
        /// </summary>
        /// <param name="photonEvent"></param>
        private void NetworkEventReceivedHandler(TransportVoiceMessage message)
        {
            var sender = MirrorNetworkActor.FromString(message.sender);

            if (!message.targetAll && sender.Id == _networkActor.Id)
                return;

            NetworkDataReceivedEvent?.Invoke(sender, message.bytes);
        }

        private void NetworkCommandEventReceivedHandler(TransportCommandMessage message)
        {
            var sender = MirrorNetworkActor.FromString(message.sender);

            //sender.SetAdminStatus(true); // NEED TO BE IMPROVED SOMEHOW BASED ON SERVER SETTINGS.

            if (!message.targetAll && sender.Id == _networkActor.Id)
                return;

            var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode(message.data);

            var stringCommand = jsonObject.ObjectValue["command"].StringValue;
            var array = jsonObject.ObjectValue["data"].ArrayValue;
            byte[] bytes = new byte[array.Count];

            int pointer = 0;
            array.ForEach((item) =>
            {
                bytes[pointer++] = item.ByteValue;
            });

            NetworkRouter.NetworkCommand command = new NetworkRouter.NetworkCommand((NetworkRouter.NetworkCommand.Command)Enum.Parse(typeof(NetworkRouter.NetworkCommand.Command), stringCommand), bytes);

            NetworkCommandReceivedEvent?.Invoke(sender, command);
        }

        [Serializable]
        public class MirrorNetworkActor : INetworkActor
        {
            public string Id => Info.id;

            public string Name => Info.name;

            public bool IsAdmin { get; private set; }

            public NetworkActorInfo Info { get; private set; }

            [UnityEngine.Scripting.Preserve]
            public MirrorNetworkActor(NetworkActorInfo info)
            {
                Info = info;
            }

            public void SetAdminStatus(bool status)
            {
                IsAdmin = status;
            }

            public override string ToString()
            {
                return Plugins.SimpleJSON.JSONEncoder.Encode(new Dictionary<string, object>()
                {
                    { "Id", Id },
                    { "Name", Name },
                });
            }

            public static MirrorNetworkActor FromString(string data)
            {
                var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode(data);
                return new MirrorNetworkActor(new NetworkActorInfo()
                {
                    id = jsonObject.ObjectValue["Id"].StringValue,
                    name = jsonObject.ObjectValue["Name"].StringValue
                });
            }
        }
    }

    public struct TransportVoiceMessage : NetworkMessage
    {
        public string sender;
        public byte[] bytes;
        public bool targetAll;
    }

    public struct TransportCommandMessage : NetworkMessage
    {
        public string sender;
        public string data;
        public bool targetAll;
    }
}
#endif