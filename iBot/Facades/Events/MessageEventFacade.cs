using System;
using IBot.Events;
using IBot.Facades.Events.CustomArgs;
using IBot.Misc;

namespace IBot.Facades.Events
{
    public static class MessageEventFacade
    {
        static MessageEventFacade()
        {
            ConnectionManager.BotConnectedEvent += (s, ev) => {
                UserEventManager.UserPublicMessageEvent +=
                    (o, e) => OnUserPublicMessageEvent(new PublicMessageEventArgsFacade(e.Channel, e.UserName, e.Message));
            };
        }

        public static event EventHandler<PublicMessageEventArgsFacade> UserPublicMessageEvent;

        private static void OnUserPublicMessageEvent(PublicMessageEventArgsFacade e)
            => UserPublicMessageEvent?.Invoke(null, e);
    }
}