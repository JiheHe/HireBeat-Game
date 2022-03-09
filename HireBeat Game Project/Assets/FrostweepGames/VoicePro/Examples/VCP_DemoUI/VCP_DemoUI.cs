using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.VoicePro.Examples
{
    public class VCP_DemoUI : MonoBehaviour
    {
        private List<RemoteSpeakerItem> _remoteSpeakerItems;

        public Dropdown microphonesDropdown;

        public Button refreshMicrophonesButton;

        public Toggle debugEchoToggle;

        public Toggle reliableTransmissionToggle;

        public Toggle muteRemoteClientsToggle;

        public Text stateText;

        public Text serverText;

        public Text roomNameText;

        public Transform parentOfRemoteClients;

        public Toggle muteMyClientToggle;

        public GameObject remoteClientPrefab;

        public Recorder recorder;

        public Listener listener;

        /// <summary>
        /// initializes event handlers and registers network actor
        /// </summary>
        private void Start()
        {
            refreshMicrophonesButton.onClick.AddListener(RefreshMicrophonesButtonOnClickHandler);
            muteMyClientToggle.onValueChanged.AddListener(MuteMyClientToggleValueChanged);
            muteRemoteClientsToggle.onValueChanged.AddListener(MuteRemoteClientsToggleValueChanged);
            debugEchoToggle.onValueChanged.AddListener(DebugEchoToggleValueChanged);
            reliableTransmissionToggle.onValueChanged.AddListener(ReliableTransmissionToggleValueChanged);
            microphonesDropdown.onValueChanged.AddListener(MicrophoneDropdownOnValueChanged);

            _remoteSpeakerItems = new List<RemoteSpeakerItem>();

            RefreshMicrophonesButtonOnClickHandler();

            listener.SpeakersUpdatedEvent += SpeakersUpdatedEventHandler;
        }

        /// <summary>
        /// updates speakers and info in ui
        /// </summary>
        private void Update()
        {
            stateText.text = "Client state: " + NetworkRouter.Instance.GetNetworkState();
            serverText.text = "Server: " + NetworkRouter.Instance.GetConnectionToServer();
            roomNameText.text = "Room: " + NetworkRouter.Instance.GetCurrentRoomName();

            foreach (var item in _remoteSpeakerItems)
            {
                item.Update();
            }
        }

        /// <summary>
        /// handler of event that updates list of speakers in ui
        /// </summary>
        /// <param name="speakers"></param>
        private void SpeakersUpdatedEventHandler(List<Speaker> speakers)
        {
            if(_remoteSpeakerItems.Count > 0)
            {
                for(int i =0; i < _remoteSpeakerItems.Count; i++)
                {
                    if (!speakers.Contains(_remoteSpeakerItems[i].Speaker))
                    {
                        _remoteSpeakerItems[i].Dispose();
                        _remoteSpeakerItems.RemoveAt(i--);
                    }
                }
            }

            foreach(var speaker in speakers)
			{
                if(_remoteSpeakerItems.Find(item => item.Speaker == speaker) == null)
				{
                    _remoteSpeakerItems.Add(new RemoteSpeakerItem(parentOfRemoteClients, remoteClientPrefab, speaker));
                }
            }
        }

        /// <summary>
        /// refreshes list of microphones. works async in webgl
        /// </summary>
        private void RefreshMicrophonesButtonOnClickHandler()
        {
            recorder.RefreshMicrophones();

            microphonesDropdown.ClearOptions();
            microphonesDropdown.AddOptions(CustomMicrophone.devices.ToList());

            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                recorder.SetMicrophone(CustomMicrophone.devices[0]);
            }
        }

        /// <summary>
        /// sets status of recording of my mic
        /// </summary>
        /// <param name="status"></param>
        private void MuteMyClientToggleValueChanged(bool status)
        {
            if (status)
            {
                if (!NetworkRouter.Instance.ReadyToTransmit || !recorder.StartRecord())
				{
                    muteMyClientToggle.isOn = false;
                }
            }
            else
            {
                recorder.StopRecord();
            }
        }

        /// <summary>
        /// mutes all speakers connected to listener
        /// </summary>
        /// <param name="status"></param>
        private void MuteRemoteClientsToggleValueChanged(bool status)
        {
            listener.SetMuteStatus(status);
        }

        /// <summary>
        ///  sets debug echo network parameter
        /// </summary>
        /// <param name="status"></param>
        private void DebugEchoToggleValueChanged(bool status)
        {
            recorder.debugEcho = status;
        }

        /// <summary>
        /// sets reliable network transmission parameter
        /// </summary>
        /// <param name="status"></param>
        private void ReliableTransmissionToggleValueChanged(bool status)
        {
            GeneralConfig.Config.reliableTransmission = status;
        }

        /// <summary>
        /// updates mic device in recorder
        /// </summary>
        /// <param name="index"></param>
        private void MicrophoneDropdownOnValueChanged(int index)
		{
            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                recorder.SetMicrophone(CustomMicrophone.devices[index]);
            }
        }

        /// <summary>
        /// ui element for showing speaker
        /// </summary>
        private class RemoteSpeakerItem
        {
            private GameObject _selfObject;

            private Text _speakerNameText;

            private Toggle _muteToggle;

            private Toggle _notTalkingToggle;

            private Toggle _muteUserMicToggle;

            public Speaker Speaker { get; private set; }

            /// <summary>
            /// initializer of speaker bsed on constructor
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="prefab"></param>
            /// <param name="speaker"></param>
            public RemoteSpeakerItem(Transform parent, GameObject prefab, Speaker speaker)
            {
                Speaker = speaker;
                _selfObject = Instantiate(prefab, parent, false);
                _speakerNameText = _selfObject.transform.Find("Text").GetComponent<Text>();
                _muteToggle = _selfObject.transform.Find("Remote IsTalking").GetComponent<Toggle>();
                _notTalkingToggle = _selfObject.transform.Find("Remote_NotTalking").GetComponent<Toggle>();
                _muteUserMicToggle = _selfObject.transform.Find("Remote_MuteMic").GetComponent<Toggle>();

                _speakerNameText.text = Speaker.Name;

                _muteToggle.onValueChanged.AddListener(MuteToggleValueChangedEventHandler);
                _notTalkingToggle.onValueChanged.AddListener(MuteToggleNotTalkingValueChangedEventHandler);
                _muteUserMicToggle.onValueChanged.AddListener(MuteUserMicToggleValueChangedEventHandler);
            }

            /// <summary>
            /// sets status of toggles bases on speaker parameter
            /// </summary>
            public void Update()
			{
                _notTalkingToggle.gameObject.SetActive(!Speaker.Playing);
                _muteToggle.gameObject.SetActive(Speaker.Playing);
            }

            /// <summary>
            /// cleanups itself
            /// </summary>
            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }

            /// <summary>
            /// sets status of mute of speaker
            /// </summary>
            /// <param name="value"></param>
            private void MuteToggleValueChangedEventHandler(bool value)
            {
                if (!_muteToggle.gameObject.activeInHierarchy)
                    return;

                Speaker.IsMute = value;

                _notTalkingToggle.isOn = value;
            }


            /// <summary>
            /// sets status of talk of speaker
            /// </summary>
            /// <param name="value"></param>
            private void MuteToggleNotTalkingValueChangedEventHandler(bool value)
            {
                if (!_notTalkingToggle.gameObject.activeInHierarchy)
                    return;

                Speaker.IsMute = value;

                _muteToggle.isOn = value;
            }

            private void MuteUserMicToggleValueChangedEventHandler(bool value)
			{
                AdminTools.SetSpeakerMuteStatus(Speaker, !value);
            }
        }
    }
}