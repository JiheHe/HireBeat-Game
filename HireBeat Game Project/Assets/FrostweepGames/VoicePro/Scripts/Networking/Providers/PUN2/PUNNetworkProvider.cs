#if PUN2_NETWORK_PROVIDER
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrostweepGames.VoicePro.NetworkProviders.PUN
{
    /// <summary>
    /// Photon PUN 2 Network Provider for data transmition
    /// </summary>
    public class PUNNetworkProvider : INetworkProvider
    {
        /// <summary>
        /// Reserved Code of network event that uses for voice data transition
        /// </summary>
        private const byte VoiceEventCode = 199;
        /// <summary>
        /// Reserved Code of network event that uses for command data transition
        /// </summary>
        private const byte CommandEventCode = 198;

        public event Action<INetworkActor, byte[]> NetworkDataReceivedEvent;

        public event Action<INetworkActor, NetworkRouter.NetworkCommand> NetworkCommandReceivedEvent;

        private INetworkActor _networkActor;

        private GameObject _eventsHandler;

        public bool ReadyToTransmit => PhotonNetwork.NetworkClientState == ClientState.Joined;

        public INetworkActor NetworkActor => _networkActor;

        public void Dispose()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkEventReceivedHandler;

            _networkActor = null;
            NetworkDataReceivedEvent = null;

            if (_eventsHandler != null)
            {
                MonoBehaviour.Destroy(_eventsHandler);
                _eventsHandler = null;
            }
        }

        public void Init(INetworkActor networkActor)
        {
            if (GeneralConfig.Config.useInternalImplementationOfNetwork)
            {
                _eventsHandler = MonoBehaviour.Instantiate(Resources.Load<GameObject>("PhotonNetworkInstance"));
            }

            _networkActor = networkActor;           

            PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;
			PhotonNetwork.NetworkingClient.StateChanged += StateChangedHandler;
        }

		public void SendNetworkData(NetworkRouter.NetworkParameters parameters, byte[] bytes)
        {
            // sending data of recorded samples by using raise event feature
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = parameters.sendToAll ? ReceiverGroup.All : ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = parameters.reliable };
            PhotonNetwork.RaiseEvent(VoiceEventCode, new object[] { _networkActor.ToString(), bytes }, raiseEventOptions, sendOptions);
        }

        public void SendCommandData(NetworkRouter.NetworkCommand command)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers =  ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true, DeliveryMode = DeliveryMode.ReliableUnsequenced };
            PhotonNetwork.RaiseEvent(CommandEventCode, new object[] { 
                _networkActor.ToString(),
                Plugins.SimpleJSON.JSONEncoder.Encode(new Dictionary<string, object>()
                {
                    { "command", command.command.ToString() },
                    { "data", command.data },
                })
            }, raiseEventOptions, sendOptions);
        }

        public string GetNetworkState()
        {
            return PhotonNetwork.NetworkClientState.ToString();
        }

        public string GetConnectionToServer()
        {
            return PhotonNetwork.Server.ToString();
        }

        public string GetCurrentRoomName()
        {
            return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : string.Empty;
        }

        /// <summary>
        /// Uses for setting of admin rights
        /// </summary>
        /// <returns></returns>
        private bool SetAdminOfRoom()
		{
            if (!ReadyToTransmit)
                return false;

            if (PhotonNetwork.NetworkingClient.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "VoicePro_Admin_ID", _networkActor.Id } });
                ((PUNNetworkActor)NetworkActor).SetAdminStatus(true);
            }

            return true;
        }

        private void StateChangedHandler(ClientState arg1, ClientState state)
        {
            if (state == ClientState.Joined)
            {
                SetAdminOfRoom();
            }
        }

        /// <summary>
		/// PUN event handler of network events
		/// </summary>
		/// <param name="photonEvent"></param>
		private void NetworkEventReceivedHandler(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case VoiceEventCode:
                    {
                        object[] data = (object[])photonEvent.CustomData;

                        string sender = (string)data[0];
                        byte[] bytes = (byte[])data[1];

                        var senderObject = PUNNetworkActor.FromString(sender);
                        senderObject.SetAdminStatus((string)PhotonNetwork.CurrentRoom.CustomProperties["VoicePro_Admin_ID"] == senderObject.Id);

                        NetworkDataReceivedEvent?.Invoke(senderObject, bytes);
                    }
                    break;
                case CommandEventCode:
					{
                        object[] data = (object[])photonEvent.CustomData;

                        string sender = (string)data[0];

                        var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode((string)data[1]);

                        var stringCommand = jsonObject.ObjectValue["command"].StringValue;
                        var array = jsonObject.ObjectValue["data"].ArrayValue;
                        byte[] bytes = new byte[array.Count];

                        int pointer = 0;
                        array.ForEach((item) =>
                        {
                            bytes[pointer++] = item.ByteValue;
                        });

                        NetworkRouter.NetworkCommand command = new NetworkRouter.NetworkCommand((NetworkRouter.NetworkCommand.Command)Enum.Parse(typeof(NetworkRouter.NetworkCommand.Command), stringCommand), bytes);

                        var senderObject = PUNNetworkActor.FromString(sender);
                        senderObject.SetAdminStatus((string)PhotonNetwork.CurrentRoom.CustomProperties["VoicePro_Admin_ID"] == senderObject.Id);

                        NetworkCommandReceivedEvent?.Invoke(senderObject, command);
                    }
                    break;
            }         
        }

        [Serializable]
        public class PUNNetworkActor : INetworkActor
        {
            public string Id => Info.id;

            public string Name => Info.name;

            public bool IsAdmin { get; private set; }

            public NetworkActorInfo Info { get; private set; }

            [UnityEngine.Scripting.Preserve]
            public PUNNetworkActor(NetworkActorInfo info)
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

            public static PUNNetworkActor FromString(string data)
            {
                var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode(data);
                return new PUNNetworkActor(new NetworkActorInfo()
                {
                    id = jsonObject.ObjectValue["Id"].StringValue,
                    name = jsonObject.ObjectValue["Name"].StringValue
                });
            }
        }
    }
}
#endif