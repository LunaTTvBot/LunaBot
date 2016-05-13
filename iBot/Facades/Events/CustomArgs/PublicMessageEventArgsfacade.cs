namespace IBot.Facades.Events.CustomArgs
{
    public class PublicMessageEventArgsFacade
    {
        public PublicMessageEventArgsFacade(string channel, string user, string msg)
        {
            Channel = channel;
            User = user;
            Msg = msg;
        }

        public string Channel { get; }
        public string User { get; }
        public string Msg { get; }
    }
}