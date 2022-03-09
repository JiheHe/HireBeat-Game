using System;
using System.Collections.Generic;
#if PUN2_NETWORK_PROVIDER
using FrostweepGames.VoicePro.NetworkProviders.PUN;
#endif
#if MIRROR_NETWORK_PROVIDER
using FrostweepGames.VoicePro.NetworkProviders.Mirror;
#endif

namespace FrostweepGames.VoicePro
{
	public class NetworkRouter
	{
		private const string Unknown = "Unknown";

		/// <summary>
		/// Network event handler that raises when network data recieved
		/// </summary>
		public event Action<INetworkActor, byte[]> NetworkDataReceivedEvent;

		private static NetworkRouter _Instance;

		/// <summary>
		/// Instance of a network router
		/// </summary>
		public static NetworkRouter Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new NetworkRouter();
				return _Instance;
			}
		}

		/// <summary>
		/// Current network provider instance
		/// </summary>
		private INetworkProvider _networkProvider;

		public bool ReadyToTransmit => _networkProvider.ReadyToTransmit;

		public List<INetworkActor> MutedNetworkActors { get; private set; } = new List<INetworkActor>();

        static NetworkRouter()
        {
			string id = GetUniqueUserId();
			Instance.Register(new NetworkActorInfo()
			{
				id = id,
				name = $"User_{id}"
			});
		}

		/// <summary>
		/// Registers user in network, but first registers network
		/// </summary>
		/// <param name="id">user id</param>
		/// <param name="name">user name</param>
		/// <param name="networkType">type of the network will use</param>
		private void Register(NetworkActorInfo info)
		{
			INetworkActor networkActor = null;

			switch (GeneralConfig.Config.networkProvider)
			{
				case GeneralConfig.NetworkProvider.PUN2_NETWORK_PROVIDER:
					{
#if PUN2_NETWORK_PROVIDER
						_networkProvider = new PUNNetworkProvider();
						networkActor = new PUNNetworkProvider.PUNNetworkActor(info);
#endif
					}
					break;
				case GeneralConfig.NetworkProvider.MIRROR_NETWORK_PROVIDER:
					{
#if MIRROR_NETWORK_PROVIDER
						_networkProvider = new MirrorNetworkProvider();
						networkActor = new MirrorNetworkProvider.MirrorNetworkActor(info);
#endif
					}
					break;
                default:
					{
						UnityEngine.Debug.LogException(new NotImplementedException($"Network didn't registered! Network provider didn't implemented. {GeneralConfig.Config.networkProvider}; Please select valid one in GeneralConfig."));
						return;
					}
					
			}

			if (_networkProvider == null)
			{
				UnityEngine.Debug.LogException(new Exception("Network Provider isnt registered. Check selected Network Provider in General Config."));
				return;
			}

			_networkProvider.Init(networkActor);
			_networkProvider.NetworkCommandReceivedEvent += NetworkCommandReceivedEventHandler;
			_networkProvider.NetworkDataReceivedEvent += NetworkDataReceivedEventHandler;
		}

		/// <summary>
		/// Sends data over network based on selected NetworkType
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="data"></param>
		public void SendNetworkData(NetworkParameters parameters, byte[] data)
		{
			if (_networkProvider == null)
				throw new NullReferenceException("Network didn't registered! Try call Register function first.");

			_networkProvider.SendNetworkData(parameters, data);
		}

		/// <summary>
		/// Sends command over network based on selected NetworkType
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="data"></param>
		public void SendCommandData(NetworkCommand command)
		{
			if (_networkProvider == null)
				throw new NullReferenceException("Network didn't registered! Try call Register function first.");

			_networkProvider.SendCommandData(command);
		}

		/// <summary>
		/// Unregisters and destroys current network user and network connection at all
		/// </summary>
		private void Unregister()
		{
			_networkProvider?.Dispose();
			_networkProvider = null;
		}

		/// <summary>
		/// Returns current network state based on connection
		/// </summary>
		/// <returns></returns>
		public string GetNetworkState()
		{
			return _networkProvider != null ? _networkProvider.GetNetworkState() : Unknown;
		}

		/// <summary>
		/// Returns current connection state bsed on server
		/// </summary>
		/// <returns></returns>
		public string GetConnectionToServer()
		{
			return _networkProvider != null ? _networkProvider.GetConnectionToServer() : Unknown;
		}

		/// <summary>
		/// Returns name of a room user connected to
		/// </summary>
		/// <returns></returns>
		public string GetCurrentRoomName()
		{
			return _networkProvider != null ? _networkProvider.GetCurrentRoomName() : Unknown;
		}

		public bool IsClientMuted()
		{
			return _networkProvider != null ? MutedNetworkActors.Contains(_networkProvider.NetworkActor) : false;
		}

		public bool IsClientAdmin()
		{
			return _networkProvider != null ? _networkProvider.NetworkActor.IsAdmin : false;
		}

		/// <summary>
		/// Handler of receiving data from different networks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void NetworkDataReceivedEventHandler(INetworkActor sender, byte[] data)
		{
			NetworkDataReceivedEvent?.Invoke(sender, data);
		}

		/// <summary>
		/// Handler of receiving commands from different networks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void NetworkCommandReceivedEventHandler(INetworkActor sender, NetworkCommand networkCommand)
		{
			switch (networkCommand.command)
			{
				case NetworkCommand.Command.MuteUser:
					{
						if (sender.IsAdmin)
						{
							string jsonData = BytesToString(networkCommand.data);
							var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode(jsonData);

							INetworkActor networkActor = null;
#if PUN2_NETWORK_PROVIDER
							networkActor = new PUNNetworkProvider.PUNNetworkActor(new NetworkActorInfo()
							{
								id = jsonObject.ObjectValue["Id"].StringValue,
								name = jsonObject.ObjectValue["Name"].StringValue
							});

#elif MIRROR_NETWORK_PROVIDER
							networkActor = new MirrorNetworkProvider.MirrorNetworkActor(new NetworkActorInfo()
							{
								id = jsonObject.ObjectValue["Id"].StringValue,
								name = jsonObject.ObjectValue["Name"].StringValue
							});
#endif

							if (networkActor != null)
							{
								if (_networkProvider.NetworkActor.Id == networkActor.Id)
								{
									if (!MutedNetworkActors.Contains(_networkProvider.NetworkActor))
										MutedNetworkActors.Add(_networkProvider.NetworkActor);
								}
								else
								{
									if (MutedNetworkActors.Find(item => item.Id == networkActor.Id) == null)
										MutedNetworkActors.Add(networkActor);
								}
							}
						}
					}
						break;
					case NetworkCommand.Command.UnmuteUser:
					{
						if (sender.IsAdmin)
						{
							string jsonData = BytesToString(networkCommand.data);
							var jsonObject = Plugins.SimpleJSON.JSONDecoder.Decode(jsonData);

							INetworkActor networkActor = null;
#if PUN2_NETWORK_PROVIDER

							networkActor = new PUNNetworkProvider.PUNNetworkActor(new NetworkActorInfo()
							{
								id = jsonObject.ObjectValue["Id"].StringValue,
								name = jsonObject.ObjectValue["Name"].StringValue
							});
#elif MIRROR_NETWORK_PROVIDER
							networkActor = new MirrorNetworkProvider.MirrorNetworkActor(new NetworkActorInfo()
							{
								id = jsonObject.ObjectValue["Id"].StringValue,
								name = jsonObject.ObjectValue["Name"].StringValue
							});
#endif

							if (networkActor != null)
							{
								if (_networkProvider.NetworkActor.Id == networkActor.Id)
								{
									if (MutedNetworkActors.Contains(_networkProvider.NetworkActor))
										MutedNetworkActors.Remove(_networkProvider.NetworkActor);
								}
								else
								{
									var foundActor = MutedNetworkActors.Find(item => item.Id == networkActor.Id);
									if (foundActor != null)
										MutedNetworkActors.Remove(foundActor);
								}
							}
						}
					}
					break;
				case NetworkCommand.Command.Unknown:
				default:
					throw new NotImplementedException($"Unknown command has received from sender: {sender.Id}");
			}
		}

		private string BytesToString(byte[] data)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}

		private static string GetUniqueUserId()
        {
			return Guid.NewGuid().ToString();
        }

		public class NetworkParameters
		{
			public bool sendToAll;

			public bool reliable;

			[UnityEngine.Scripting.Preserve]
			public NetworkParameters()
			{
			}

			[UnityEngine.Scripting.Preserve]
			public NetworkParameters(bool sendToAll, bool reliable)
			{
				this.sendToAll = sendToAll;
				this.reliable = reliable;
			}
		}

		[Serializable]
		public class NetworkCommand
		{
			public readonly Command command;
			public readonly byte[] data;

			[UnityEngine.Scripting.Preserve]
			public NetworkCommand(Command command, byte[] data)
			{
				this.command = command;
				this.data = data;
			}

			public enum Command
			{
				Unknown,

				MuteUser,
				UnmuteUser,
			}
		}
	}
}