namespace IBot.Facades.Core
{
    public enum ConnectionChangeType : short
    {
        Connected = 1 << 0,
        Disconnected = 1 << 1,
    }
}