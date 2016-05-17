using System.Collections.Generic;

namespace iBot_GUI.Forms.TabControls.StartPageFolder
{
    public class ConnectionDetails
    {
        public ConnectionDetails(ICollection<string> channelList, string token, string nick)
        {
            ChannelList = channelList;
            Token = token;
            Nick = nick;
        }

        public string Nick { get; set; }

        public string Token { get; set; }

        public ICollection<string> ChannelList { get; }
    }
}