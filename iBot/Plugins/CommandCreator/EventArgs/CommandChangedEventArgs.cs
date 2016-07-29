namespace IBot.Plugins.CommandCreator.EventArgs
{
    internal class CommandChangedEventArgs
    {
        public CommandCreator Command { get; }

        public CommandChangedEventArgs(CommandCreator command)
        {
            Command = command;
        }
    }
}