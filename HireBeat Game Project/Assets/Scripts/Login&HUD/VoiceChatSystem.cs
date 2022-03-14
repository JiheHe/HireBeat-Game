using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using FrostweepGames.MicrophonePro;

namespace FrostweepGames.VoicePro.Examples
{
    public class VoiceChatSystem : MonoBehaviour
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

        string myID;

        /// <summary>
        /// initializes event handlers and registers network actor
        /// </summary>
        private void Start()
        {
            /*myID = GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID;

            refreshMicrophonesButton.onClick.AddListener(RefreshMicrophonesButtonOnClickHandler);
            muteMyClientToggle.onValueChanged.AddListener(MuteMyClientToggleValueChanged);
            muteRemoteClientsToggle.onValueChanged.AddListener(MuteRemoteClientsToggleValueChanged);
            debugEchoToggle.onValueChanged.AddListener(DebugEchoToggleValueChanged);
            reliableTransmissionToggle.onValueChanged.AddListener(ReliableTransmissionToggleValueChanged);
            microphonesDropdown.onValueChanged.AddListener(MicrophoneDropdownOnValueChanged);

            _remoteSpeakerItems = new List<RemoteSpeakerItem>();

            RefreshMicrophonesButtonOnClickHandler();

            listener.SpeakersUpdatedEvent += SpeakersUpdatedEventHandler;*/

            RefreshMicrophonesButtonOnClickHandler();
        }

        //Also custom... need to be prepped beforehand by the acctive  script changeReceiver.
        public void InitializationSteps()
        {
            myID = GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID;

            refreshMicrophonesButton.onClick.AddListener(RefreshMicrophonesButtonOnClickHandler);
            muteMyClientToggle.onValueChanged.AddListener(MuteMyClientToggleValueChanged);
            muteRemoteClientsToggle.onValueChanged.AddListener(MuteRemoteClientsToggleValueChanged);
            debugEchoToggle.onValueChanged.AddListener(DebugEchoToggleValueChanged);
            reliableTransmissionToggle.onValueChanged.AddListener(ReliableTransmissionToggleValueChanged);
            microphonesDropdown.onValueChanged.AddListener(MicrophoneDropdownOnValueChanged);

            _remoteSpeakerItems = new List<RemoteSpeakerItem>();

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

        //no need for this! Just grab directly from data base, then new network actor instance grabs from there when generated.
        //and in terms of real time update, more efficient to update the display directly anyway.
        /*public void UpdatePersonalNetworkRouterName(string name) 
        {
            NetworkRouter.Instance._networkProvider.NetworkActor.SetNetworkActorInfoName(name); //hopefully updates local.
        }*/

        /// <summary>
        /// handler of event that updates list of speakers in ui
        /// </summary>
        /// <param name="speakers"></param>
        private void SpeakersUpdatedEventHandler(List<Speaker> speakers)
        {
            if (_remoteSpeakerItems.Count > 0)
            {
                for (int i = 0; i < _remoteSpeakerItems.Count; i++)
                {
                    if (!speakers.Contains(_remoteSpeakerItems[i].Speaker))
                    {
                        _remoteSpeakerItems[i].Dispose();
                        _remoteSpeakerItems.RemoveAt(i--);
                    }
                }
            }

            foreach (var speaker in speakers)
            {
                //if(speaker.Id == myID) listener.SpeakerLeave(myID); //added ID comparison, to avoid adding self in //THIS ONLY HAPPENS IF DEBUG ECHO IS PRESSED!!!!

                if (_remoteSpeakerItems.Find(item => item.Speaker == speaker) == null)
                {
                    var prefabWithID = Instantiate(remoteClientPrefab);
                    prefabWithID.GetComponent<GenerateInfoCardOnClick>().userID = speaker.Id;
                    _remoteSpeakerItems.Add(new RemoteSpeakerItem(parentOfRemoteClients, prefabWithID, speaker));
                    //_remoteSpeakerItems.Add(new RemoteSpeakerItem(parentOfRemoteClients, remoteClientPrefab, speaker)); //this line is the original
                }
            }
        }

        //this method is a custom one i wrote
        public void CheckCurrentSpeakerNames()
        {
            var CurrPlayersInRoom = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            foreach (var speakerItem in _remoteSpeakerItems)
            {
                var itemSpeaker = speakerItem.Speaker;
                var target = CurrPlayersInRoom.Find(p => p.UserId == itemSpeaker.Id); //find matching id people
                if (target != null && target.NickName != itemSpeaker.Name) //shouldn't be null unless bug //when current name is diff from rec
                {
                    speakerItem.UpdateDisplayName(target.NickName);
                }
            }
        }

        string previousMicCache; //added this to keep track of prev mic
        /// <summary>
        /// refreshes list of microphones. works async in webgl
        /// </summary>
        private void RefreshMicrophonesButtonOnClickHandler()
        {
            previousMicCache = recorder.GetMicrophoneDeviceName(); //for ex, this stores "Microphone 1"
            muteMyClientToggle.isOn = false; //turn off the mic while transitioning mic

            recorder.RefreshMicrophones();

            microphonesDropdown.ClearOptions();
            microphonesDropdown.AddOptions(CustomMicrophone.devices.ToList());

            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                if(previousMicCache != CustomMicrophone.devices[0] && !recorder.StopRecordWithMicName(previousMicCache))
                {
                    recorder.recording = false; //if stop recording is false, then maually set var to ready!
                }
                recorder.SetMicrophone(CustomMicrophone.devices[0]);
            }
        }

        /// <summary>
        /// sets status of recording of my mic
        /// </summary>
        /// <param name="status"></param>
        public void MuteMyClientToggleValueChanged(bool status)
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

            /*if (status)
            {
                bool resultRecorder = !recorder.StartRecord(); //this will call and return yay or nay
                bool resultNetwork = !NetworkRouter.Instance.ReadyToTransmit;
                if (resultNetwork || resultRecorder) //wait this is an or.. since this statement passed, one of them is true for it to work!
                {
                    muteMyClientToggle.isOn = false; //this line is only executed when error.
                    //for this error prevention line to execute, one of the two bool is false.
                }
                if(resultNetwork) Debug.LogError("Network instance not ready to transmit"); 
                if (resultRecorder) Debug.LogError("Recorder cannot start recording!"); //problem identified: recorder...
            }
            else
            {
                recorder.StopRecord();
            }*/
        }

        /// <summary>
        /// mutes all speakers connected to listener
        /// </summary>
        /// <param name="status"></param>
        public void MuteRemoteClientsToggleValueChanged(bool status)
        {
            listener.SetMuteStatus(status);
        }

        /// <summary>
        ///  sets debug echo network parameter
        /// </summary>
        /// <param name="status"></param>
        public void DebugEchoToggleValueChanged(bool status)
        {
            recorder.debugEcho = status;

            if(status == false) //added this: if you leave debug, then remove your obj
            {
                ClearSpeaker(myID);
                Invoke("WaitAndClearSelf", 0.5f);
            }
        }

        private void WaitAndClearSelf() //this is to deal with the case that the remaining of self tab msg gets through and form new tab
        {
            ClearSpeaker(myID);
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
            previousMicCache = recorder.GetMicrophoneDeviceName(); //for ex, this stores "Microphone 1"
            muteMyClientToggle.isOn = false; //turn off the mic while transitioning mic

            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                //Debug.LogError("Successfully stopped recording at dropdown?: " + recorder.StopRecord());
                if (previousMicCache != CustomMicrophone.devices[index] && !recorder.StopRecordWithMicName(previousMicCache))
                {
                    recorder.recording = false; //if stop recording is false, then maually set var to ready!
                }
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

            //made this up as well ;D
            public void UpdateDisplayName(string name)
            {
                _speakerNameText.text = name;
            }
        }

        //some of my own custom methods
        public void ClearSpeaker(string id)
        {
            //NetworkRouter.Instance. //_networkProvider.Dispose(); //Unregister(); //This leaves the photon room... we don't want that...
            //listener.SpeakerLeave(GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID); //this only removes the speaker item of yourself...
            listener.SpeakerLeave(id);
        }

        public void ChangeNetworkInfoName(string name)
        {
            NetworkRouter.Instance.ChangeNetworkInfoName(name);
        }

        public void OnOtherPlayerConnected(string id)
        {
            listener.CreatePlayerJoined(id);
        }
    }
}