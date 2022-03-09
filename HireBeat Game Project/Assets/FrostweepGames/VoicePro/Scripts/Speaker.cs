using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using UnityEngine;

namespace FrostweepGames.VoicePro
{
    /// <summary>
    /// Basic speaker of sound
    /// </summary>
    public class Speaker
    {
        private GameObject _selfObject;

        private AudioSource _source;

        private AudioClip _workingClip;

        private Buffer _buffer;

        private bool _audioClipReadyToUse;

        private float _delay;

        private float _notActiveTime;

        private int _maxNotActiveTime;

        private bool _hasBeenDisposed;

        /// <summary>
        /// Id of as speaker
        /// </summary>
        public string Id => NetworkActor.Info.id;

        /// <summary>
        /// name of a speaker
        /// </summary>
        public string Name => NetworkActor.Info.name;

        /// <summary>
        /// Does speaker muted or not
        /// </summary>
        public bool IsMute { get { return _source.mute; } set { _source.mute = value; } }

        /// <summary>
        /// Is client active or not. if inactive it will be destroyed and marked as inactive. look at _maxNotActiveTime regarding max time of inactivity
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Returns state of speaker - does it says something or not
        /// </summary>
        public bool Playing { get; private set; }

        /// <summary>
        /// Returns NetworkActor of a speaker
        /// </summary>
        public INetworkActor NetworkActor { get; private set; }

        public Speaker(INetworkActor networkActor, GameObject parent)
        {
            NetworkActor = networkActor;

            _selfObject = new GameObject(Name);
            _source = _selfObject.AddComponent<AudioSource>();

            _hasBeenDisposed = false;
            _buffer = new Buffer();

            SetObjectOwner(parent);
            ApplyConfig(GeneralConfig.Config.speakerConfig);
            InitSound();

            IsActive = true;         
        }

        /// <summary>
        /// Gives a possibility to attach speaker on any object in 3D space
        /// </summary>
        /// <param name="gameObject"></param>
        public void SetObjectOwner(GameObject gameObject)
        {
            if (_hasBeenDisposed)
            {
                throw new System.Exception("Trying to set an object owner on disposed Speaker object.");
            }

            _selfObject.transform.SetParent(gameObject?.transform, false);
        }

        /// <summary>
        /// Refreshing speaker settings based on config
        /// </summary>
        /// <param name="config"></param>
        public void ApplyConfig(SpeakerConfig config)
        {
            if (_hasBeenDisposed)
            {
                throw new System.Exception("Trying to apply a speaker config on disposed Speaker object.");
            }

            _source.spatialBlend = config.spatialBlend;
            _source.outputAudioMixerGroup = config.outputAudioMixerGroup;
            _maxNotActiveTime = config.maxInactiveTime;
        }

        /// <summary>
        /// Calls every frame
        /// </summary>
        internal void Update()
        {
            if (_hasBeenDisposed)
                return;

            ProcessAudio();
        }

        /// <summary>
        /// Fill samples buffer from data from network converted to float array
        /// </summary>
        /// <param name="bytes"></param>
        internal void HandleRawData(byte[] bytes)
        {
            _buffer.data.AddRange(CustomMicrophone.ByteToFloat(bytes));
            _notActiveTime = 0f;
        }

        /// <summary>
        /// Destroys this speaker object with cleaning data
        /// </summary>
        internal void Dispose()
        {
            if (_hasBeenDisposed)
                return;

            _source.Stop(); 
            _buffer.data.Clear();
            _buffer.position = 0;

            if (_workingClip != null)
            {
                Object.Destroy(_workingClip);
            }
            Object.Destroy(_selfObject);
            _selfObject = null;

            _hasBeenDisposed = true;
        }

        /// <summary>
        /// Initializes AudioClip and sets it to audio source
        /// </summary>
        private void InitSound()
		{
            if (_workingClip != null || _workingClip)
                MonoBehaviour.Destroy(_workingClip);

            _workingClip = AudioClip.Create("BufferedClip_" + Id, Constants.SampleRate * Constants.RecordingTime, 1, Constants.SampleRate, false);
            _source.clip = _workingClip;
        }

        /// <summary>
        /// Do whole processing of playing data from network
        /// </summary>
        private void ProcessAudio()
        {
            try
            {
                _audioClipReadyToUse = _buffer.data.Count > 0;

                if (Playing)
                {
                    _delay -= Time.deltaTime;

                    if (_delay <= 0)
                    {
                        _source.Stop();
                        //InitSound();
                        Playing = false;
                    }
                }

                if (!Playing)
                {
                    if (_audioClipReadyToUse)
                    {
                        List<float> chunk;

                        if (_buffer.data.Count >= Constants.SampleRate)
                        {
                            chunk = _buffer.data.GetRange(0, Constants.SampleRate);
                            _buffer.data.RemoveRange(0, Constants.SampleRate);

                            _delay = Constants.RecordingTime;
                        }
                        else
                        {
                            int bufferSize = _buffer.data.Count;

                            chunk = new List<float>();
                            chunk.AddRange(_buffer.data);
                            _buffer.data.Clear();

                            for (int i = bufferSize; i < Constants.SampleRate; i++)
                                chunk.Add(0);

                            _delay = (float)bufferSize / (float)Constants.SampleRate;
                        }

                        float[] chunkArray = chunk.ToArray();

                        if (GeneralConfig.Config.echoCancellation)
                        {
                            EchoCancellation.Instance.RegisterFramePlayed(chunkArray);
                        }

                        _workingClip.SetData(chunkArray, 0);
                        _source.Play();

                        Playing = true;
                    }

                    _notActiveTime += Time.deltaTime;
                }

                IsActive = _notActiveTime < _maxNotActiveTime;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Speaker exception: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Basic data buffer of samples
        /// </summary>
        private class Buffer
        {
            public int position;
            public List<float> data;

            [UnityEngine.Scripting.Preserve]
            public Buffer()
            {
                position = 0;
                data = new List<float>();
            }
        }
    }
}