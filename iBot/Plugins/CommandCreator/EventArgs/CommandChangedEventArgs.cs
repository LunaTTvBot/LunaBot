namespace IBot.Plugins.CommandCreator.EventArgs
{
    internal class CommandChangedEventArgs
    {
        public CommandCreatorPlugin Command { get; }

        public CommandChangedEventArgs(CommandCreatorPlugin command)
        {
            Command = command;
        }
    }
}