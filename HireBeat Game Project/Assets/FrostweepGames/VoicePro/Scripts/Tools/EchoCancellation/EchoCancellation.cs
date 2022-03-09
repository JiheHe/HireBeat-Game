using System;
using FrostweepGames.Plugins.Native;
using FrostweepGames.VoicePro.DSP.WebRTC;

namespace FrostweepGames.VoicePro
{
	public class EchoCancellation
	{
		private static EchoCancellation _Instance;
		public static EchoCancellation Instance
		{
			get
			{
				if(_Instance == null)
				{
					_Instance = new EchoCancellation();
				}
				return _Instance;
			}
		}

		private WebRtcFilter _enhancer;

		private AudioFormat _audioFormat;

		/// <summary>
		/// Initializes WebRtcFilter
		/// </summary>
		public EchoCancellation()
		{
			_audioFormat = new AudioFormat(Constants.SampleRate, Constants.ChunkTime, Constants.Channels);

			_enhancer = new WebRtcFilter(Constants.ChunkTime, 
										 Constants.ChunkTime,
										 _audioFormat,
										 _audioFormat,
										 GeneralConfig.Config.echoCancellationEnableAec,
										 GeneralConfig.Config.echoCancellationEnableDenoise,
										 GeneralConfig.Config.echoCancellationEnableAgc);
		}

		/// <summary>
		/// Call this when you play frame to speakers
		/// </summary>
		/// <param name="bytes"></param>
		public void RegisterFramePlayed(float[] samples)
		{
			_enhancer.RegisterFramePlayed(CustomMicrophone.FloatToByte(samples));
		}

		/// <summary>
		/// call this when you get data from mic before sending to network
		/// </summary>
		/// <param name="bytes"></param>
		public void RegisterFrameRecorded(float[] samples)
		{
			_enhancer.Write(CustomMicrophone.FloatToByte(samples));
		}

		/// <summary>
		/// Process frames to send them over the network
		/// </summary>
		public void GetProcessEchoCancellationFrame(Action<float[]> frameToSendCallback)
		{
			bool moreFrames;
			do
			{
				short[] cancelBuffer = new short[_audioFormat.SamplesPerFrame]; // contains cancelled audio signal
				if (_enhancer.Read(cancelBuffer, out moreFrames))
				{
					byte[] bytes = new byte[_audioFormat.BytesPerFrame];
					Buffer.BlockCopy(cancelBuffer, 0, bytes, 0, _audioFormat.BytesPerFrame);
					frameToSendCallback?.Invoke(CustomMicrophone.ByteToFloat(bytes));
				}
			} while (moreFrames);
		}
	}
}
