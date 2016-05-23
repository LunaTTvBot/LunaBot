using System.Collections.Generic;
using System.Net;
using IBot.Core;
using IBot.Core.Settings;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace IBot.TwitchAPI
{
    public static class Twitch
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private const string TwitchApiBase = "https://api.twitch.tv/kraken";
        private const string GetChannelSubscriptions = "/channels/{0}/subscriptions";
        private const string GetChannelFollowers = "/channels/{0}/follows";

        /// <summary>
        /// client_id    = your client ID
        /// redirect_uri = your registered redirect URI
        /// scope        = space separated list of scopes
        /// state        = your provided unique token
        /// </summary>
        private const string UserAuth = "/oauth2/authorize?response_type=token&client_id={0}&redirect_uri={1}&scope={2}";

        private static readonly RestClient Client;

        static Twitch()
        {
            Client = new RestClient(TwitchApiBase);
        }

        private static string CallTwitch(string url)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
            //request.AddHeader("Client-ID", SettingsManager.GetSettings<ConnectionSettings>().ApplicationId);
            request.AddHeader("Authorization", SettingsManager.GetSettings<ConnectionSettings>().OwnerTwitchApiKey.Replace("oauth:", "OAuth "));

            var response = Client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content;
            }

            _logger.Error("Http: {0}, Message: {1}", response.StatusCode, response.ErrorMessage);
            return "";
        }

        public static IEnumerable<string> GetChannelSubscribers(string channel)
        {
            //generate class from json
            var json = CallTwitch(string.Format(GetChannelSubscriptions, channel));

            try
            {
                //var retVal = JsonConvert.DeserializeObject<>()
            }
            catch (JsonException e) {}

            return new List<string>();
        }
    }
}
