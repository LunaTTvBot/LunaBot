using IRCConnectionTest.TmiApi.ChannelChattersEndpoint;
using RestSharp;

namespace IRCConnectionTest.TmiApi
{
    internal static class TmiApi
    {
        private static readonly RestClient Client;

        static TmiApi()
        {
            Client = new RestClient(TmiUriProvider.TmiBaseUri);
        }

        public static ChannelChatters GetChannelChatters(string channel)
        {
            var request = new RestRequest(TmiUriProvider.GetChannelChattersUri(channel), Method.GET);
            var response = Client.Execute<ChannelChatters>(request);

            return response.Data;
        }
    }
}