namespace IBot.Plugins
{
    public interface IPlugin
    {
        string PluginName { get; }
        void Init();
    }
}