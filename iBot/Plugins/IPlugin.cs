namespace IBot.Plugins
{
    internal interface IPlugin
    {
        string PluginName { get; }
        void Init();
    }
}