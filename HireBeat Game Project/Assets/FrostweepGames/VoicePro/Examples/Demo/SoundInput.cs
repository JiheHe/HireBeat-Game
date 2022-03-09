using FrostweepGames.Plugins.Native;
using FrostweepGames.VoicePro;
using UnityEngine;

namespace FrostweepGames.VoicePro.Examples
{
    public class SoundInput : MonoBehaviour
    {
        public Recorder recorder;

        public Listener listener;

        public bool isRecording;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && !isRecording)
            {
                StartRecord();
            }
            else if (Input.GetKeyUp(KeyCode.R) && isRecording)
            {
                StopRecord();
            }
        }

        public void StartRecord()
        {
            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                recorder.SetMicrophone(CustomMicrophone.devices[0]);
                isRecording = recorder.StartRecord();

                Debug.Log("Record started: " + isRecording);
            }
			else
			{
                recorder.RefreshMicrophones();
            }  
        }

        public void StopRecord()
        {
            isRecording = false;
            recorder.StopRecord();

            Debug.Log("Record ended");
        }
    }
}