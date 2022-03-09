#if MIRROR_NETWORK_PROVIDER
using Mirror;
#endif

namespace FrostweepGames.VoicePro.NetworkProviders.Mirror
{
    public class VoiceNetworkManager 
#if MIRROR_NETWORK_PROVIDER
        : NetworkManager
#else
        : UnityEngine.MonoBehaviour
#endif
    {
        /// <summary>
        /// determines is it server or client
        /// </summary>
        [UnityEngine.Header("Voice Network Manager Parameters")]
        public bool isServer;

#if MIRROR_NETWORK_PROVIDER

		private new void Start()
		{
			if (isServer)
			{
                StartServer();
			}
			else
			{
                StartClient();
			}
		}

		public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<TransportVoiceMessage>(NetworkEventReceivedHandler);

            UnityEngine.Debug.Log("OnStartServer");
        }

        public override void OnStopServer()
		{
			base.OnStopServer();
			NetworkServer.UnregisterHandler<TransportVoiceMessage>();

            UnityEngine.Debug.Log("OnStopServer");
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            // clients connected to server
            UnityEngine.Debug.Log("OnClientConnect");
        }

        public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

            // clients disconnected from server

            UnityEngine.Debug.Log("OnClientDisconnect");
        }

        private void NetworkEventReceivedHandler(NetworkConnection connection, TransportVoiceMessage message)
        {
            NetworkServer.SendToReady(message);
        }
#endif
    }
}