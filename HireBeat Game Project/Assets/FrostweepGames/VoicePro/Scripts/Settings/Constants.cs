namespace FrostweepGames.VoicePro
{
    /// <summary>
    /// Collects all constants in one class
    /// </summary>
    public class Constants 
    {
        /// <summary>
        /// How long will be recorded voice
        /// </summary>
        public const int RecordingTime = 1;

        /// <summary>
        /// Default sample rate of microphone
        /// </summary>
        public const int SampleRate = 16000;

        /// <summary>
        /// Channels usabe when recording
        /// </summary>
        public const int Channels = 1; // support 1 channel in WebGL

        /// <summary>
        /// Chunk time im miliseconds
        /// </summary>
        public const int ChunkTime = 150;

        /// <summary>
        /// Size of block that sends over network
        /// </summary>
        public const int ChunkSize = (int)(SampleRate * (double)((double)ChunkTime / 1000d)); // send with interval ~ ChunkTime ms
    }
}