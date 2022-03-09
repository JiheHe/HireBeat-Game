namespace FrostweepGames.VoicePro
{
    public partial class DefineProcessing : Plugins.DefineProcessing
    {
        internal static readonly string[] _Defines = new string[] 
        {
            "FG_VOICEPRO"
        };

        static DefineProcessing()
        {
            AddOrRemoveDefines(true, false, _Defines);
        }
    }
}