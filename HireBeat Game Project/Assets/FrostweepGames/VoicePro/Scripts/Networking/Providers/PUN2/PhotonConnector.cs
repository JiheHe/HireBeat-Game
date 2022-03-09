using UnityEngine;

namespace FrostweepGames.VoicePro.NetworkProviders.PUN
{
    [RequireComponent(typeof(PhotonLobby))]
    public class PhotonConnector : MonoBehaviour
    {
        private PhotonLobby _lobby;

        private float _connectTimer;

#if PUN2_NETWORK_PROVIDER
        private void Awake()
        {
            _lobby = GetComponent<PhotonLobby>();

            _lobby.ConnectedEvent += ConnectedEventHandler;
            _lobby.RoomListUpdatedEvent += RoomListUpdatedEventHandler;
            _lobby.JoinedRoomEvent += JoinedRoomEventHandler;
        }

        private void OnDestroy()
        {
            _lobby.ConnectedEvent -= ConnectedEventHandler;
            _lobby.RoomListUpdatedEvent -= RoomListUpdatedEventHandler;
            _lobby.JoinedRoomEvent -= JoinedRoomEventHandler;

            _lobby.Disconnect();
        }

        private void Start()
        {
            _lobby.Connect((SystemInfo.deviceUniqueIdentifier + Random.Range(0, 9999999)).GetHashCode().ToString());
        }

		private void Update()
		{
			if(_connectTimer > 0)
			{
                _connectTimer -= Time.deltaTime;

                if (_connectTimer <= 0f)
                {
                    if (_lobby.Rooms.Count == 0)
                    {
                        _lobby.CreateRoom();
                    }
                    else
                    {
                        _lobby.JoinRoom();
                    }
                }
            }
		}

		private void ConnectedEventHandler()
        {
            _lobby.JoinLobby();
        }

        private void RoomListUpdatedEventHandler()
        {
            if (_lobby.Rooms.Count == 0)
            {
                _connectTimer = 3f;
            }
            else
            {
                _connectTimer = 0f;
                _lobby.JoinRoom();
            }
        }

        private void JoinedRoomEventHandler()
        {
        }

#endif
    }
}