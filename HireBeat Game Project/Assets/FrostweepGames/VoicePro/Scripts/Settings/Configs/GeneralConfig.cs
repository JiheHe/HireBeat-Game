using UnityEngine;

namespace FrostweepGames.VoicePro
{
    //[CreateAssetMenu(fileName = "GeneralConfig", menuName = "FrostweepGames/VoicePro/GeneralConfig", order = 3)]
    public class GeneralConfig : ScriptableObject
    {
        private static GeneralConfig _Config;
        public static GeneralConfig Config
        {
            get
            {
                if (_Config == null)
                    _Config = GetConfig();
                return _Config;
            }
        }

        public bool showWelcomeDialogAtStartup = true;

        /// <summary>
        /// Current selected network provider
        /// </summary>
        public NetworkProvider networkProvider = NetworkProvider.Unknown;

        /// <summary>
        /// Sets detecting of voice (could decrease performance)
        /// </summary>
        public bool voiceDetectionEnabled = false;

        /// <summary>
        /// Sets voice volume threshold for detection of voice
        /// </summary>
        [Range(0f, 0.5f)]
        public float voiceDetectionThreshold = 0.02f;

        /// <summary>
        /// Sets if transmission over network will be reliable or not
        /// </summary>
        public bool reliableTransmission = true;

        /// <summary>
        /// Configures of usage of internal plugin network implementation, or usage of custom. If set to true - uses internal network implementation.
        /// </summary>
        public bool useInternalImplementationOfNetwork = true;

        /// <summary>
        /// Configures of compressing of data which transfers
        /// </summary>
        public bool compressingOfTrasferDataEnabled = true;

        /// <summary>
        /// Configures of usage of echo cancellation feature
        /// </summary>
        public bool echoCancellation = false;

        public bool echoCancellationEnableAec = true;

        public bool echoCancellationEnableDenoise = Constants.ChunkTime == 10; // only support 10ms

        public bool echoCancellationEnableAgc = true;

        /// <summary>
        /// Configuration of speakers
        /// </summary>
        public SpeakerConfig speakerConfig;

        private static GeneralConfig GetConfig()
        {
            string path = "VoicePro/GeneralConfig";
            var config = Resources.Load<GeneralConfig>(path);

            if(config == null)
            {
                Debug.LogError($"Voice Pro General Config not found in {path} Resources folder. Will use default.");

                config = (GeneralConfig)CreateInstance("GeneralConfig");

#if UNITY_EDITOR
                string pathToFolder = "Assets/Frostweepgames/VoicePro/Resources/VoicePro";
                string filename = "GeneralConfig.asset";

                if (!System.IO.Directory.Exists(Application.dataPath + "/../" + pathToFolder))
                {
                    System.IO.Directory.CreateDirectory(pathToFolder);
                    UnityEditor.AssetDatabase.ImportAsset(pathToFolder);
                }

                if (!System.IO.File.Exists(Application.dataPath + "/../" + pathToFolder + "/" + filename))
                {
                    UnityEditor.AssetDatabase.CreateAsset(config, pathToFolder + "/" + filename);
                }
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }

            return config;
        }


        public enum NetworkProvider
        {
            Unknown,

            PUN2_NETWORK_PROVIDER,
            MIRROR_NETWORK_PROVIDER
        }
    }
}
