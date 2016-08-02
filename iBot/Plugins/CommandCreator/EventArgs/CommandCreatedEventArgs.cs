namespace IBot.Plugins.CommandCreator.EventArgs
{
    internal class CommandCreatedEventArgs
    {
        public CommandCreatedEventArgs(CommandCreatorPlugin command)
        {
            Command = command;
        }

        public CommandCreatorPlugin Command { get; }
    }
}
