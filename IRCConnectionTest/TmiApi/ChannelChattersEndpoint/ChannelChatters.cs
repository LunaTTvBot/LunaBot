using RestSharp.Deserializers;

namespace IRCConnectionTest.TmiApi.ChannelChattersEndpoint
{
    internal class ChannelChatters
    {
        [DeserializeAs(Name = "_links")]
        public object Links { get; set; }

        [DeserializeAs(Name = "chatter_count")]
        public int Count { get; set; }

        [DeserializeAs(Name = "chatters")]
        public Chatters Chatters { get; set; }
    }
}