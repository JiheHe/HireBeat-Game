namespace FrostweepGames.VoicePro
{
    public static class AdminTools
    {
        public static bool SetSpeakerMuteStatus(Speaker speaker, bool isMute)
        {
            if (NetworkRouter.Instance.IsClientAdmin())
            {
                NetworkRouter.Instance.SendCommandData(new NetworkRouter.NetworkCommand(
                    isMute ? NetworkRouter.NetworkCommand.Command.MuteUser : NetworkRouter.NetworkCommand.Command.UnmuteUser,
                    System.Text.Encoding.UTF8.GetBytes(speaker.NetworkActor.ToString())));
                return true;
            }
            return false;
		}
    }
}