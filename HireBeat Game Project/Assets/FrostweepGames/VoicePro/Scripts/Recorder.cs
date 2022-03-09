using UnityEngine;
using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System;
using FrostweepGames.Plugins;

namespace FrostweepGames.VoicePro
{
	/// <summary>
	/// Basic record system for voice chat
	/// </summary>
	public class Recorder : MonoBehaviour
	{
		/// <summary>
		/// Throws when record successfully started
		/// </summary>
		public event Action RecordStartedEvent;

		/// <summary>
		/// Throws when record successfully ended
		/// </summary>
		public event Action RecordEndedEvent;

		/// <summary>
		/// Throws when record starting failed
		/// </summary>
		public event Action<string> RecordFailedEvent;

		/// <summary>
		/// Last cached sample position
		/// </summary>
		private int _lastPosition = 0;

		/// <summary>
		/// Array of recoreded samples
		/// </summary>
		private List<float> _buffer;

		/// <summary>
		/// Microphone audio clip
		/// </summary>
		private AudioClip _workingClip;

		/// <summary>
		/// Current selected microphone device in usage
		/// </summary>
		[ReadOnly]
		[SerializeField]
		private string _microphoneDevice;

		/// <summary>
		/// RAW samples from microphone
		/// </summary>
		private float[] _rawSamples;

		/// <summary>
		/// Average voice level during recording
		/// </summary>
		[ReadOnly]
		[SerializeField]
		private float _averageVoiceLevel = 0f;

		/// <summary>
		/// Saves last position of mic when it stops
		/// </summary>
		private int _stopRecordPosition = -1;

		/// <summary>
		/// Average voice level during recording
		/// </summary>
		[ReadOnly]
		[SerializeField]
		private bool _isMuted = false;

		/// <summary>
		/// Says status of recording
		/// </summary>
		[ReadOnly]
		public bool recording = false;

		/// <summary>
		/// Sets network receivers in network, if enabled then sends also on this client, if not - only others
		/// </summary>
		public bool debugEcho = false;

		/// <summary>
		/// Initializes buffer, refreshes microphones list and selects first microphone device if exists
		/// </summary>
		private void Start()
		{
			_buffer = new List<float>();
			_rawSamples = new float[Constants.RecordingTime * Constants.SampleRate];

			RefreshMicrophones();

			if (CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				_microphoneDevice = CustomMicrophone.devices[0];
			}
		}

		/// <summary>
		/// Handles processing of recording each frame
		/// </summary>
		private void Update()
		{
			_isMuted = NetworkRouter.Instance.IsClientMuted();

			if (!string.IsNullOrEmpty(_microphoneDevice))
			{
				ProcessRecording();
			}
		}

		/// <summary>
		/// Processes samples data from microphone recording and fills buffer of samples then sends it over network
		/// </summary>
		private void ProcessRecording()
		{
			int currentPosition = CustomMicrophone.GetPosition(_microphoneDevice);

			// fix for end record incorrect position
			if (_stopRecordPosition != -1)
				currentPosition = _stopRecordPosition;

			if ((recording || currentPosition != _lastPosition) && !_isMuted)
			{
				if (CustomMicrophone.GetRawData(ref _rawSamples, _workingClip))
				{
					if (_lastPosition != currentPosition && _rawSamples.Length > 0)
					{
						// Detects does user says something based on volume level
						if (!GeneralConfig.Config.voiceDetectionEnabled || CustomMicrophone.IsVoiceDetected(_rawSamples, ref _averageVoiceLevel, GeneralConfig.Config.voiceDetectionThreshold))
						{
							if (_lastPosition > currentPosition)
							{
								_buffer.AddRange(GetChunk(_rawSamples, _lastPosition, _rawSamples.Length - _lastPosition));
								_buffer.AddRange(GetChunk(_rawSamples, 0, currentPosition));
							}
							else
							{
								_buffer.AddRange(GetChunk(_rawSamples, _lastPosition, currentPosition - _lastPosition));
							}
						}

						// sends data chunky
						if (_buffer.Count >= Constants.ChunkSize)
						{
							SendDataToNetwork(_buffer.GetRange(0, Constants.ChunkSize));
							_buffer.RemoveRange(0, Constants.ChunkSize);
						}
					}
				}

				_lastPosition = currentPosition;
			}
			else
			{
				_lastPosition = currentPosition;

				if (_buffer.Count > 0)
				{
					// sends left data chunky
					if (_buffer.Count >= Constants.ChunkSize)
					{
						SendDataToNetwork(_buffer.GetRange(0, Constants.ChunkSize));
						_buffer.RemoveRange(0, Constants.ChunkSize);
					}
					// sends all left data
					else
					{
						SendDataToNetwork(_buffer);
						_buffer.Clear();
					}
				}
			}
		}

		/// <summary>
		/// Gets range from an array based on start index and length
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">input array</param>
		/// <param name="index">start offset</param>
		/// <param name="length">length of output array and how many items will be copied from initial array</param>
		/// <returns></returns>
		private T[] GetChunk<T> (T[] data, int index, int length)
		{
			if(data.Length < index + length)
				throw new Exception("Input array less than parameters income!");

			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		/// <summary>
		/// Sends data to other clients or if debug echo then sends to all including this client
		/// </summary>
		/// <param name="samples">list of sampels that will be sent over network</param>
		private void SendDataToNetwork(List<float> samples)
		{
			float[] chunk = samples.ToArray();

			Action<float[]> action = (array) =>
			{
				byte[] bytes = CustomMicrophone.FloatToByte(array);

				NetworkRouter.Instance.SendNetworkData(new NetworkRouter.NetworkParameters()
				{
					reliable = GeneralConfig.Config.reliableTransmission,
					sendToAll = debugEcho
				}, GeneralConfig.Config.compressingOfTrasferDataEnabled ? Compressor.Compress(bytes) : bytes);
			};

			if (GeneralConfig.Config.echoCancellation)
			{
				EchoCancellation.Instance.RegisterFrameRecorded(chunk);
				EchoCancellation.Instance.GetProcessEchoCancellationFrame(action);
			}
			else
			{
				action(chunk);
			}
		}

		/// <summary>
		/// Requests microphone perission and refreshes list of microphones if WebGL platform
		/// </summary>
		public void RefreshMicrophones()
		{
			CustomMicrophone.RequestMicrophonePermission();
			CustomMicrophone.RefreshMicrophoneDevices();
		}

		/// <summary>
		/// Starts recording of microphone
		/// </summary>
		public bool StartRecord()
		{
			if (CustomMicrophone.IsRecording(_microphoneDevice) || !CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				RecordFailedEvent?.Invoke("record already started or no microphone device connected");
				return false;
			}

			if (recording)
			{
				RecordFailedEvent?.Invoke("record already started");
				return false;
			}

			_stopRecordPosition = -1;

			recording = true;

			_buffer?.Clear();

			_workingClip = CustomMicrophone.Start(_microphoneDevice, true, Constants.RecordingTime, Constants.SampleRate);

			RecordStartedEvent?.Invoke();

			return true;
		}

		/// <summary>
		/// Stops recording of microphone
		/// </summary>
		public bool StopRecord()
		{
			if (!CustomMicrophone.IsRecording(_microphoneDevice))
				return false;

			if (!recording)
				return false;

			recording = false;

			if (CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				_stopRecordPosition = CustomMicrophone.GetPosition(_microphoneDevice);

				CustomMicrophone.End(_microphoneDevice, () =>
				{
					if (_workingClip != null)
					{
						Destroy(_workingClip);
					}

					RecordEndedEvent?.Invoke();
				});
			}
			else
			{
				if (_workingClip != null)
				{
					Destroy(_workingClip);
				}

				RecordEndedEvent?.Invoke();
			}

			return true;
		}

		/// <summary>
		/// Sets microphone device for usage
		/// </summary>
		/// <param name="microphone"></param>
		public void SetMicrophone(string microphone)
		{
			_microphoneDevice = microphone;
		}
	}
}