using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FrostweepGames.VoicePro
{
	/// <summary>
	/// Basic listener system for voice chat
	/// </summary>
	public class Listener : MonoBehaviour
	{
		public event Action<List<Speaker>> SpeakersUpdatedEvent;

		public event Action<string> SpeakerLeavedByInactiveEvent;

		private object _lock = new object();

		private bool _listening;

		/// <summary>
		/// Sets if listening of netowrk events should be started at awake
		/// </summary>
		public bool startListenOnAwake = true;

		/// <summary>
		/// Returns key - value pair : id of a speaker and its object instance
		/// </summary>
		public Dictionary<string, Speaker> Speakers { get; private set; }

		/// <summary>
		/// Returns info about does speakers muted or not
		/// </summary>
		public bool IsSpeakersMuted { get; private set; } = false;

		private void Awake()
		{
			Speakers = new Dictionary<string, Speaker>();

			if(startListenOnAwake)
			{
				StartListen();
			}
		}

		private void OnDestroy()
		{
			StopListen();

			ResetSpeakers();
		}

		private void LateUpdate()
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.Update();
				}
			}

			CleanInactiveSpeakers();
		}

		/// <summary>
		/// Resets and destroys all active speakers
		/// </summary>
		private void ResetSpeakers()
		{
			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.Dispose();
				}
				Speakers.Clear();
			}
		}

		/// <summary>
		/// cleans inactive speakers
		/// </summary>
		private void CleanInactiveSpeakers()
		{
			lock (_lock)
			{
				List<string> inactive = new List<string>();

				foreach (var speaker in Speakers)
				{
					if(!speaker.Value.IsActive)
					{
						inactive.Add(speaker.Key);
					}
				}

				foreach(string id in inactive)
				{
					Speakers[id].Dispose();
					Speakers.Remove(id);
				}

				if(inactive.Count > 0)
				{				
					SpeakersUpdatedEvent?.Invoke(Speakers.Values.ToList());
				}

				inactive.Clear();
			}
		}

		/// <summary>
		/// Handles data from network connected to specific client id
		/// </summary>
		/// <param name="id">unique id of a remote client</param>
		/// <param name="bytes">array of received data (samples)</param>
		private void HandleRawData(INetworkActor sender, byte[] bytes)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				Speaker speaker;

				if (!Speakers.ContainsKey(sender.Id))
				{
					speaker = new Speaker(sender, gameObject);
					speaker.IsMute = IsSpeakersMuted;

					Speakers.Add(sender.Id, speaker);

					SpeakersUpdatedEvent?.Invoke(Speakers.Values.ToList());
				}
				else
				{
					speaker = Speakers[sender.Id];
				}

				speaker.HandleRawData(bytes);
			}
		}

		/// <summary>
		/// Network data event handler
		/// </summary>
		/// <param name="sender">network sender</param>
		/// <param name="data">transmission data</param>
		private void NetworkDataReceivedEventHandler(INetworkActor sender, byte[] data)
		{
			HandleRawData(sender, GeneralConfig.Config.compressingOfTrasferDataEnabled ? Compressor.Decompress(data) : data);
		}

		/// <summary>
		/// Starts listening of network events
		/// </summary>
		public void StartListen()
		{
			if (_listening)
				return;

			NetworkRouter.Instance.NetworkDataReceivedEvent += NetworkDataReceivedEventHandler;

			_listening = true;
		}

		/// <summary>
		/// Stops listening of network events
		/// </summary>
		public void StopListen()
		{
			if (!_listening)
				return;

			NetworkRouter.Instance.NetworkDataReceivedEvent -= NetworkDataReceivedEventHandler;

			_listening = false;
			ResetSpeakers();
		}

		/// <summary>
		/// Disposes speaker by client id
		/// </summary>
		/// <param name="id"></param>
		public void SpeakerLeave(string id)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				if (Speakers.ContainsKey(id))
				{
					Speakers[id].Dispose();
					Speakers.Remove(id);

					SpeakerLeavedByInactiveEvent?.Invoke(id);

					SpeakersUpdatedEvent?.Invoke(Speakers.Values.ToList());
				}
			}

		}

		/// <summary>
		/// Sets status of mute of all active speakers 
		/// </summary>
		/// <param name="mute"></param>
		public void SetMuteStatus(bool mute)
		{
			IsSpeakersMuted = mute;

			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.IsMute = mute;
				}
			}
		}
	}
}