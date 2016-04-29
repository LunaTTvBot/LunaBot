namespace IRCConnectionTest.Plugins
{
    public interface IPlugin
    {
        string PluginName { get; }
        void Execute();
    }
}