namespace IBot.Plugins.CommandCreator.EventArgs
{
    internal class CommandCreatedEventArgs
    {
        public CommandCreatedEventArgs(CommandCreator command)
        {
            Command = command;
        }

        public CommandCreator Command { get; }
    }
}
